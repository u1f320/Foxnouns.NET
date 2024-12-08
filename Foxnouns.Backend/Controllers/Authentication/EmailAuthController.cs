using System.Net;
using EntityFramework.Exceptions.Common;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Services.Auth;
using Foxnouns.Backend.Utils;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/internal/auth/email")]
public class EmailAuthController(
    [UsedImplicitly] Config config,
    DatabaseContext db,
    AuthService authService,
    MailService mailService,
    KeyCacheService keyCacheService,
    UserRendererService userRenderer,
    IClock clock,
    ILogger logger
) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<EmailAuthController>();

    [HttpPost("register/init")]
    public async Task<IActionResult> RegisterInitAsync(
        [FromBody] RegisterRequest req,
        CancellationToken ct = default
    )
    {
        CheckRequirements();

        if (!req.Email.Contains('@'))
            throw new ApiError.BadRequest("Email is invalid", "email", req.Email);

        string state = await keyCacheService.GenerateRegisterEmailStateAsync(req.Email, null, ct);

        // If there's already a user with that email address, pretend we sent an email but actually ignore it
        if (
            await db.AuthMethods.AnyAsync(
                a => a.AuthType == AuthType.Email && a.RemoteId == req.Email,
                ct
            )
        )
            return NoContent();

        mailService.QueueAccountCreationEmail(req.Email, state);
        return NoContent();
    }

    [HttpPost("callback")]
    public async Task<IActionResult> CallbackAsync([FromBody] CallbackRequest req)
    {
        CheckRequirements();

        RegisterEmailState? state = await keyCacheService.GetRegisterEmailStateAsync(req.State);
        if (state is not { ExistingUserId: null })
            throw new ApiError.BadRequest("Invalid state", "state", req.State);

        string ticket = AuthUtils.RandomToken();
        await keyCacheService.SetKeyAsync($"email:{ticket}", state.Email, Duration.FromMinutes(20));

        return Ok(new CallbackResponse(false, ticket, state.Email, null, null, null));
    }

    [HttpPost("register")]
    public async Task<IActionResult> CompleteRegistrationAsync(
        [FromBody] CompleteRegistrationRequest req
    )
    {
        CheckRequirements();

        string? email = await keyCacheService.GetKeyAsync($"email:{req.Ticket}");
        if (email == null)
            throw new ApiError.BadRequest("Unknown ticket", "ticket", req.Ticket);

        User user = await authService.CreateUserWithPasswordAsync(
            req.Username,
            email,
            req.Password
        );
        Application frontendApp = await db.GetFrontendApplicationAsync();

        (string? tokenStr, Token? token) = authService.GenerateToken(
            user,
            frontendApp,
            ["*"],
            clock.GetCurrentInstant() + Duration.FromDays(365)
        );
        db.Add(token);

        await db.SaveChangesAsync();

        await keyCacheService.DeleteKeyAsync($"email:{req.Ticket}");

        return Ok(
            new AuthController.AuthResponse(
                await userRenderer.RenderUserAsync(user, user, renderMembers: false),
                tokenStr,
                token.ExpiresAt
            )
        );
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthController.AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest req,
        CancellationToken ct = default
    )
    {
        CheckRequirements();

        (User? user, AuthService.EmailAuthenticationResult authenticationResult) =
            await authService.AuthenticateUserAsync(req.Email, req.Password, ct);
        if (authenticationResult == AuthService.EmailAuthenticationResult.MfaRequired)
            throw new NotImplementedException("MFA is not implemented yet");

        Application frontendApp = await db.GetFrontendApplicationAsync(ct);

        _logger.Debug("Logging user {Id} in with email and password", user.Id);

        (string? tokenStr, Token? token) = authService.GenerateToken(
            user,
            frontendApp,
            ["*"],
            clock.GetCurrentInstant() + Duration.FromDays(365)
        );
        db.Add(token);

        _logger.Debug("Generated token {TokenId} for {UserId}", token.Id, user.Id);

        await db.SaveChangesAsync(ct);

        return Ok(
            new AuthController.AuthResponse(
                await userRenderer.RenderUserAsync(user, user, renderMembers: false, ct: ct),
                tokenStr,
                token.ExpiresAt
            )
        );
    }

    [HttpPost("change-password")]
    [Authorize("*")]
    public async Task<IActionResult> UpdatePasswordAsync([FromBody] ChangePasswordRequest req)
    {
        if (!await authService.ValidatePasswordAsync(CurrentUser!, req.Current))
            throw new ApiError.Forbidden("Invalid password");

        ValidationUtils.Validate([("new", ValidationUtils.ValidatePassword(req.New))]);

        await authService.SetUserPasswordAsync(CurrentUser!, req.New);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("add-email")]
    [Authorize("*")]
    public async Task<IActionResult> AddEmailAddressAsync([FromBody] AddEmailAddressRequest req)
    {
        CheckRequirements();

        List<AuthMethod> emails = await db
            .AuthMethods.Where(m => m.UserId == CurrentUser!.Id && m.AuthType == AuthType.Email)
            .ToListAsync();
        if (emails.Count > AuthUtils.MaxAuthMethodsPerType)
        {
            throw new ApiError.BadRequest(
                "Too many email addresses, maximum of 3 per account.",
                "email",
                null
            );
        }

        if (emails.Count != 0)
        {
            if (!await authService.ValidatePasswordAsync(CurrentUser!, req.Password))
                throw new ApiError.Forbidden("Invalid password");
        }
        else
        {
            await authService.SetUserPasswordAsync(CurrentUser!, req.Password);
            await db.SaveChangesAsync();
        }

        string state = await keyCacheService.GenerateRegisterEmailStateAsync(
            req.Email,
            CurrentUser!.Id
        );

        bool emailExists = await db
            .AuthMethods.Where(m => m.AuthType == AuthType.Email && m.RemoteId == req.Email)
            .AnyAsync();
        if (emailExists)
        {
            return NoContent();
        }

        mailService.QueueAddEmailAddressEmail(req.Email, state, CurrentUser.Username);
        return NoContent();
    }

    [HttpPost("add-email/callback")]
    [Authorize("*")]
    public async Task<IActionResult> AddEmailCallbackAsync([FromBody] CallbackRequest req)
    {
        CheckRequirements();

        RegisterEmailState? state = await keyCacheService.GetRegisterEmailStateAsync(req.State);
        if (state?.ExistingUserId != CurrentUser!.Id)
            throw new ApiError.BadRequest("Invalid state", "state", req.State);

        try
        {
            AuthMethod authMethod = await authService.AddAuthMethodAsync(
                CurrentUser.Id,
                AuthType.Email,
                state.Email
            );
            _logger.Debug(
                "Added email auth {AuthId} for user {UserId}",
                authMethod.Id,
                CurrentUser.Id
            );

            return Ok(
                new AuthController.AddOauthAccountResponse(
                    authMethod.Id,
                    AuthType.Email,
                    authMethod.RemoteId,
                    null
                )
            );
        }
        catch (UniqueConstraintException)
        {
            throw new ApiError(
                "That email address is already linked.",
                HttpStatusCode.BadRequest,
                ErrorCode.AccountAlreadyLinked
            );
        }
    }

    public record AddEmailAddressRequest(string Email, string Password);

    private void CheckRequirements()
    {
        if (!config.EmailAuth.Enabled)
            throw new ApiError.BadRequest("Email authentication is not enabled on this instance.");
    }

    public record LoginRequest(string Email, string Password);

    public record RegisterRequest(string Email);

    public record CompleteRegistrationRequest(string Ticket, string Username, string Password);

    public record CallbackRequest(string State);

    public record ChangePasswordRequest(string Current, string New);
}
