using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly ILogger _logger = logger.ForContext<DiscordAuthController>();

    [HttpPost("callback")]
    // TODO: duplicating attribute doesn't work, find another way to mark both as possible response
    // leaving it here for documentation purposes
    [ProducesResponseType<AuthController.AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<AuthController.CallbackResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CallbackAsync([FromBody] AuthController.CallbackRequest req, CancellationToken ct = default)
    {
        CheckRequirements();
        await keyCacheSvc.ValidateAuthStateAsync(req.State, ct);

        var remoteUser = await remoteAuthSvc.RequestDiscordTokenAsync(req.Code, req.State, ct);
        var user = await authSvc.AuthenticateUserAsync(AuthType.Discord, remoteUser.Id, ct: ct);
        if (user != null) return Ok(await GenerateUserTokenAsync(user,ct));

        _logger.Debug("Discord user {Username} ({Id}) authenticated with no local account", remoteUser.Username,
            remoteUser.Id);

        var ticket = AuthUtils.RandomToken();
        await keyCacheSvc.SetKeyAsync($"discord:{ticket}", remoteUser, Duration.FromMinutes(20), ct);

        return Ok(new AuthController.CallbackResponse(false, ticket, remoteUser.Username));
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthController.AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterAsync([FromBody] AuthController.OauthRegisterRequest req, CancellationToken ct = default)
    {
        var remoteUser = await keyCacheSvc.GetKeyAsync<RemoteAuthService.RemoteUser>($"discord:{req.Ticket}",ct:ct);
        if (remoteUser == null) throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);
        if (await db.AuthMethods.AnyAsync(a => a.AuthType == AuthType.Discord && a.RemoteId == remoteUser.Id, ct))
        {
            _logger.Error("Discord user {Id} has valid ticket but is already linked to an existing account",
                remoteUser.Id);
            throw new FoxnounsError("Discord ticket was issued for user with existing link");
        }

        var user = await authSvc.CreateUserWithRemoteAuthAsync(req.Username, AuthType.Discord, remoteUser.Id,
            remoteUser.Username, ct: ct);

        return Ok(await GenerateUserTokenAsync(user, ct));
    }

    private async Task<AuthController.AuthResponse> GenerateUserTokenAsync(User user, CancellationToken ct = default)
    {
        var frontendApp = await db.GetFrontendApplicationAsync(ct);
        _logger.Debug("Logging user {Id} in with Discord", user.Id);

        var (tokenStr, token) =
            authSvc.GenerateToken(user, frontendApp, ["*"], clock.GetCurrentInstant() + Duration.FromDays(365));
        db.Add(token);

        _logger.Debug("Generated token {TokenId} for {UserId}", user.Id, token.Id);

        await db.SaveChangesAsync(ct);

        return new AuthController.AuthResponse(
            await userRendererSvc.RenderUserAsync(user, selfUser: user, renderMembers: false, ct: ct),
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