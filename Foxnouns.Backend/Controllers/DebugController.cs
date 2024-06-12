using Foxnouns.Backend.Controllers.Authentication;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/debug")]
public class DebugController(
    DatabaseContext db,
    AuthService authSvc,
    UserRendererService userRendererSvc,
    IClock clock,
    ILogger logger) : ApiControllerBase
{
    [HttpPost("users")]
    [ProducesResponseType<AuthController.AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequest req)
    {
        logger.Debug("Creating user with username {Username} and email {Email}", req.Username, req.Email);

        var user = await authSvc.CreateUserWithPasswordAsync(req.Username, req.Email, req.Password);
        var frontendApp = await db.GetFrontendApplicationAsync();

        var (tokenStr, token) =
            authSvc.GenerateToken(user, frontendApp, ["*"], clock.GetCurrentInstant() + Duration.FromDays(365));
        db.Add(token);

        await db.SaveChangesAsync();

        return Ok(new AuthController.AuthResponse(
            await userRendererSvc.RenderUserAsync(user, selfUser: user, renderMembers: false),
            tokenStr,
            token.ExpiresAt
        ));
    }

    public record CreateUserRequest(string Username, string Password, string Email);
}