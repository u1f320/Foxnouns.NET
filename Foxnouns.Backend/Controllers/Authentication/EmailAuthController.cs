using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
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

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterRequest req,
        CancellationToken ct = default
    )
    {
        CheckRequirements();

        if (!req.Email.Contains('@'))
            throw new ApiError.BadRequest("Email is invalid", "email", req.Email);

        var state = await keyCacheService.GenerateRegisterEmailStateAsync(
            req.Email,
            userId: null,
            ct
        );

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

        var state = await keyCacheService.GetRegisterEmailStateAsync(req.State);
        if (state == null)
            throw new ApiError.BadRequest("Invalid state", "state", req.State);

        // If this callback is for an existing user, add the email address to their auth methods
        if (state.ExistingUserId != null)
        {
            var authMethod = await authService.AddAuthMethodAsync(
                state.ExistingUserId.Value,
                AuthType.Email,
                state.Email
            );
            _logger.Debug(
                "Added email auth {AuthId} for user {UserId}",
                authMethod.Id,
                state.ExistingUserId
            );
            return NoContent();
        }

        var ticket = AuthUtils.RandomToken();
        await keyCacheService.SetKeyAsync($"email:{ticket}", state.Email, Duration.FromMinutes(20));

        return Ok(
            new AuthController.CallbackResponse(
                HasAccount: false,
                Ticket: ticket,
                RemoteUsername: state.Email,
                User: null,
                Token: null,
                ExpiresAt: null
            )
        );
    }

    [HttpPost("complete-registration")]
    public async Task<IActionResult> CompleteRegistrationAsync(
        [FromBody] CompleteRegistrationRequest req
    )
    {
        var email = await keyCacheService.GetKeyAsync($"email:{req.Ticket}");
        if (email == null)
            throw new ApiError.BadRequest("Unknown ticket", "ticket", req.Ticket);

        // Check if username is valid at all
        ValidationUtils.Validate([("username", ValidationUtils.ValidateUsername(req.Username))]);
        // Check if username is already taken
        if (await db.Users.AnyAsync(u => u.Username == req.Username))
            throw new ApiError.BadRequest("Username is already taken", "username", req.Username);

        var user = await authService.CreateUserWithPasswordAsync(req.Username, email, req.Password);
        var frontendApp = await db.GetFrontendApplicationAsync();

        var (tokenStr, token) = authService.GenerateToken(
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
                await userRenderer.RenderUserAsync(user, selfUser: user, renderMembers: false),
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

        var (user, authenticationResult) = await authService.AuthenticateUserAsync(
            req.Email,
            req.Password,
            ct
        );
        if (authenticationResult == AuthService.EmailAuthenticationResult.MfaRequired)
            throw new NotImplementedException("MFA is not implemented yet");

        var frontendApp = await db.GetFrontendApplicationAsync(ct);

        _logger.Debug("Logging user {Id} in with email and password", user.Id);

        var (tokenStr, token) = authService.GenerateToken(
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
                await userRenderer.RenderUserAsync(
                    user,
                    selfUser: user,
                    renderMembers: false,
                    ct: ct
                ),
                tokenStr,
                token.ExpiresAt
            )
        );
    }

    [HttpPost("add")]
    [Authorize("*")]
    public async Task<IActionResult> AddEmailAddressAsync([FromBody] AddEmailAddressRequest req)
    {
        var emails = await db
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

        var validPassword = await authService.ValidatePasswordAsync(CurrentUser!, req.Password);
        if (!validPassword)
        {
            throw new ApiError.Forbidden("Invalid password");
        }

        var state = await keyCacheService.GenerateRegisterEmailStateAsync(
            req.Email,
            userId: CurrentUser!.Id
        );

        var emailExists = await db
            .AuthMethods.Where(m => m.AuthType == AuthType.Email && m.RemoteId == req.Email)
            .AnyAsync();
        if (emailExists)
        {
            return NoContent();
        }

        mailService.QueueAddEmailAddressEmail(req.Email, state, CurrentUser.Username);
        return NoContent();
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
}
