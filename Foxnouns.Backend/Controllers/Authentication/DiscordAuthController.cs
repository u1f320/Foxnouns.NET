using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/internal/auth/discord")]
public class DiscordAuthController(
    [UsedImplicitly] Config config,
    ILogger logger,
    IClock clock,
    DatabaseContext db,
    KeyCacheService keyCacheService,
    AuthService authService,
    RemoteAuthService remoteAuthService,
    UserRendererService userRenderer) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<DiscordAuthController>();

    [HttpPost("callback")]
    // TODO: duplicating attribute doesn't work, find another way to mark both as possible response
    // leaving it here for documentation purposes
    [ProducesResponseType<AuthController.CallbackResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CallbackAsync([FromBody] AuthController.CallbackRequest req,
        CancellationToken ct = default)
    {
        CheckRequirements();
        await keyCacheService.ValidateAuthStateAsync(req.State, ct);

        var remoteUser = await remoteAuthService.RequestDiscordTokenAsync(req.Code, req.State, ct);
        var user = await authService.AuthenticateUserAsync(AuthType.Discord, remoteUser.Id, ct: ct);
        if (user != null) return Ok(await GenerateUserTokenAsync(user, ct));

        _logger.Debug("Discord user {Username} ({Id}) authenticated with no local account", remoteUser.Username,
            remoteUser.Id);

        var ticket = AuthUtils.RandomToken();
        await keyCacheService.SetKeyAsync($"discord:{ticket}", remoteUser, Duration.FromMinutes(20), ct);

        return Ok(new AuthController.CallbackResponse(
            HasAccount: false,
            Ticket: ticket,
            RemoteUsername: remoteUser.Username,
            User: null,
            Token: null,
            ExpiresAt: null
        ));
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthController.AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterAsync([FromBody] AuthController.OauthRegisterRequest req)
    {
        var remoteUser = await keyCacheService.GetKeyAsync<RemoteAuthService.RemoteUser>($"discord:{req.Ticket}");
        if (remoteUser == null) throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);
        if (await db.AuthMethods.AnyAsync(a => a.AuthType == AuthType.Discord && a.RemoteId == remoteUser.Id))
        {
            _logger.Error("Discord user {Id} has valid ticket but is already linked to an existing account",
                remoteUser.Id);
            throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);
        }

        var user = await authService.CreateUserWithRemoteAuthAsync(req.Username, AuthType.Discord, remoteUser.Id,
            remoteUser.Username);

        return Ok(await GenerateUserTokenAsync(user));
    }

    private async Task<AuthController.CallbackResponse> GenerateUserTokenAsync(User user,
        CancellationToken ct = default)
    {
        var frontendApp = await db.GetFrontendApplicationAsync(ct);
        _logger.Debug("Logging user {Id} in with Discord", user.Id);

        var (tokenStr, token) =
            authService.GenerateToken(user, frontendApp, ["*"], clock.GetCurrentInstant() + Duration.FromDays(365));
        db.Add(token);

        _logger.Debug("Generated token {TokenId} for {UserId}", user.Id, token.Id);

        await db.SaveChangesAsync(ct);

        return new AuthController.CallbackResponse(
            HasAccount: true,
            Ticket: null,
            RemoteUsername: null,
            User: await userRenderer.RenderUserAsync(user, selfUser: user, renderMembers: false, ct: ct),
            Token: tokenStr,
            ExpiresAt: token.ExpiresAt
        );
    }

    private void CheckRequirements()
    {
        if (!config.DiscordAuth.Enabled)
            throw new ApiError.BadRequest("Discord authentication is not enabled on this instance.");
    }
}