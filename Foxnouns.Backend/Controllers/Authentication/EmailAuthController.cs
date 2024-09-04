using Foxnouns.Backend.Database;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/v2/auth/email")]
public class EmailAuthController(
    DatabaseContext db,
    AuthService authSvc,
    UserRendererService userRendererSvc,
    IClock clock,
    ILogger logger) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<EmailAuthController>();

    [HttpPost("login")]
    [ProducesResponseType<AuthController.AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest req)
    {
        var (user, authenticationResult) = await authSvc.AuthenticateUserAsync(req.Email, req.Password);
        if (authenticationResult == AuthService.EmailAuthenticationResult.MfaRequired)
            throw new NotImplementedException("MFA is not implemented yet");

        var frontendApp = await db.GetFrontendApplicationAsync();

        _logger.Debug("Logging user {Id} in with email and password", user.Id);

        var (tokenStr, token) =
            authSvc.GenerateToken(user, frontendApp, ["*"], clock.GetCurrentInstant() + Duration.FromDays(365));
        db.Add(token);

        _logger.Debug("Generated token {TokenId} for {UserId}", token.Id, user.Id);

        await db.SaveChangesAsync();

        return Ok(new AuthController.AuthResponse(
            await userRendererSvc.RenderUserAsync(user, selfUser: user, renderMembers: false),
            tokenStr,
            token.ExpiresAt
        ));
    }

    public record LoginRequest(string Email, string Password);
}