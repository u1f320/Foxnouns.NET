using Foxnouns.Backend.Database;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/v2/auth/email")]
public class EmailAuthController(DatabaseContext db, AuthService authSvc, UserRendererService userRendererSvc, IClock clock, ILogger logger) : ApiControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest req)
    {
        var user = await authSvc.AuthenticateUserAsync(req.Email, req.Password);
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

    public record LoginRequest(string Email, string Password);
}