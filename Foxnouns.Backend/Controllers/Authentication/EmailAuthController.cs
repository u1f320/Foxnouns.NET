using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/v2/auth/email")]
public class EmailAuthController(
    DatabaseContext db,
    AuthService authSvc,
    MailService mailSvc,
    KeyCacheService keyCacheSvc,
    UserRendererService userRendererSvc,
    IClock clock,
    ILogger logger) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<EmailAuthController>();

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest req, CancellationToken ct = default)
    {
        if (!req.Email.Contains('@')) throw new ApiError.BadRequest("Email is invalid", "email", req.Email);

        var state = await keyCacheSvc.GenerateRegisterEmailStateAsync(req.Email, userId: null, ct);
        if (await db.AuthMethods.AnyAsync(a => a.AuthType == AuthType.Email && a.RemoteId == req.Email, ct))
            return NoContent();

        mailSvc.QueueAccountCreationEmail(req.Email, state);
        return NoContent();
    }

    [HttpPost("callback")]
    public async Task<IActionResult> CallbackAsync([FromBody] CallbackRequest req, CancellationToken ct = default)
    {
        var state = await keyCacheSvc.GetRegisterEmailStateAsync(req.State, ct);
        if (state == null) throw new ApiError.BadRequest("Invalid state", "state", req.State);

        if (state.ExistingUserId != null)
        {
            var authMethod =
                await authSvc.AddAuthMethodAsync(state.ExistingUserId.Value, AuthType.Email, state.Email, ct: ct);
            _logger.Debug("Added email auth {AuthId} for user {UserId}", authMethod.Id, state.ExistingUserId);
            return NoContent();
        }

        var ticket = AuthUtils.RandomToken();
        await keyCacheSvc.SetKeyAsync($"email:{ticket}", state.Email, Duration.FromMinutes(20));

        return Ok(new AuthController.CallbackResponse(false, ticket, state.Email));
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthController.AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest req, CancellationToken ct = default)
    {
        var (user, authenticationResult) = await authSvc.AuthenticateUserAsync(req.Email, req.Password, ct);
        if (authenticationResult == AuthService.EmailAuthenticationResult.MfaRequired)
            throw new NotImplementedException("MFA is not implemented yet");

        var frontendApp = await db.GetFrontendApplicationAsync(ct);

        _logger.Debug("Logging user {Id} in with email and password", user.Id);

        var (tokenStr, token) =
            authSvc.GenerateToken(user, frontendApp, ["*"], clock.GetCurrentInstant() + Duration.FromDays(365));
        db.Add(token);

        _logger.Debug("Generated token {TokenId} for {UserId}", token.Id, user.Id);

        await db.SaveChangesAsync(ct);

        return Ok(new AuthController.AuthResponse(
            await userRendererSvc.RenderUserAsync(user, selfUser: user, renderMembers: false, ct: ct),
            tokenStr,
            token.ExpiresAt
        ));
    }

    public record LoginRequest(string Email, string Password);

    public record RegisterRequest(string Email);

    public record CallbackRequest(string State);
}