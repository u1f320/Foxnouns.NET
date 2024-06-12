using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/v2/auth/discord")]
public class DiscordAuthController(
    Config config,
    ILogger logger,
    IClock clock,
    DatabaseContext db,
    KeyCacheService keyCacheSvc,
    AuthService authSvc,
    RemoteAuthService remoteAuthSvc,
    UserRendererService userRendererSvc) : ApiControllerBase
{
    [HttpPost("callback")]
    public async Task<IActionResult> CallbackAsync([FromBody] AuthController.CallbackRequest req)
    {
        CheckRequirements();
        await keyCacheSvc.ValidateAuthStateAsync(req.State);

        var remoteUser = await remoteAuthSvc.RequestDiscordTokenAsync(req.Code, req.State);
        var user = await authSvc.AuthenticateUserAsync(AuthType.Discord, remoteUser.Id);
        if (user != null) return Ok(await GenerateUserTokenAsync(user));

        logger.Debug("Discord user {Username} ({Id}) authenticated with no local account", remoteUser.Username,
            remoteUser.Id);

        throw new NotImplementedException();
    }

    private async Task<AuthController.AuthResponse> GenerateUserTokenAsync(User user)
    {
        var frontendApp = await db.GetFrontendApplicationAsync();
        logger.Debug("Logging user {Id} in with Discord", user.Id);

        var (tokenStr, token) =
            authSvc.GenerateToken(user, frontendApp, ["*"], clock.GetCurrentInstant() + Duration.FromDays(365));
        db.Add(token);

        logger.Debug("Generated token {TokenId} for {UserId}", user.Id, token.Id);

        await db.SaveChangesAsync();

        return new AuthController.AuthResponse(
            await userRendererSvc.RenderUserAsync(user, selfUser: user, renderMembers: false),
            tokenStr,
            token.ExpiresAt
        );
    }

    private void CheckRequirements()
    {
        if (!config.DiscordAuth.Enabled)
            throw new ApiError.BadRequest("Discord authentication is not enabled on this instance.");
    }
}