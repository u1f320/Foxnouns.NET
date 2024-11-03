using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Services.Auth;
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
    DatabaseContext db,
    KeyCacheService keyCacheService,
    AuthService authService,
    RemoteAuthService remoteAuthService
) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<DiscordAuthController>();

    [HttpPost("callback")]
    [ProducesResponseType<CallbackResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CallbackAsync([FromBody] AuthController.CallbackRequest req)
    {
        CheckRequirements();
        await keyCacheService.ValidateAuthStateAsync(req.State);

        var remoteUser = await remoteAuthService.RequestDiscordTokenAsync(req.Code);
        var user = await authService.AuthenticateUserAsync(AuthType.Discord, remoteUser.Id);
        if (user != null)
            return Ok(await authService.GenerateUserTokenAsync(user));

        _logger.Debug(
            "Discord user {Username} ({Id}) authenticated with no local account",
            remoteUser.Username,
            remoteUser.Id
        );

        var ticket = AuthUtils.RandomToken();
        await keyCacheService.SetKeyAsync(
            $"discord:{ticket}",
            remoteUser,
            Duration.FromMinutes(20)
        );

        return Ok(
            new CallbackResponse(
                HasAccount: false,
                Ticket: ticket,
                RemoteUsername: remoteUser.Username,
                User: null,
                Token: null,
                ExpiresAt: null
            )
        );
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthController.AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] AuthController.OauthRegisterRequest req
    )
    {
        var remoteUser = await keyCacheService.GetKeyAsync<RemoteAuthService.RemoteUser>(
            $"discord:{req.Ticket}"
        );
        if (remoteUser == null)
            throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);
        if (
            await db.AuthMethods.AnyAsync(a =>
                a.AuthType == AuthType.Discord && a.RemoteId == remoteUser.Id
            )
        )
        {
            _logger.Error(
                "Discord user {Id} has valid ticket but is already linked to an existing account",
                remoteUser.Id
            );
            throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);
        }

        var user = await authService.CreateUserWithRemoteAuthAsync(
            req.Username,
            AuthType.Discord,
            remoteUser.Id,
            remoteUser.Username
        );

        return Ok(await authService.GenerateUserTokenAsync(user));
    }

    private void CheckRequirements()
    {
        if (!config.DiscordAuth.Enabled)
            throw new ApiError.BadRequest(
                "Discord authentication is not enabled on this instance."
            );
    }
}
