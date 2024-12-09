using System.Net;
using System.Web;
using EntityFramework.Exceptions.Common;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
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

[Route("/api/internal/auth/tumblr")]
public class TumblrAuthController(
    [UsedImplicitly] Config config,
    ILogger logger,
    DatabaseContext db,
    KeyCacheService keyCacheService,
    AuthService authService,
    RemoteAuthService remoteAuthService
) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<TumblrAuthController>();

    [HttpPost("callback")]
    [ProducesResponseType<CallbackResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CallbackAsync([FromBody] CallbackRequest req)
    {
        CheckRequirements();
        await keyCacheService.ValidateAuthStateAsync(req.State);

        RemoteAuthService.RemoteUser remoteUser = await remoteAuthService.RequestTumblrTokenAsync(
            req.Code
        );
        User? user = await authService.AuthenticateUserAsync(AuthType.Tumblr, remoteUser.Id);
        if (user != null)
            return Ok(await authService.GenerateUserTokenAsync(user));

        _logger.Debug(
            "Tumblr user {Username} ({Id}) authenticated with no local account",
            remoteUser.Username,
            remoteUser.Id
        );

        string ticket = AuthUtils.RandomToken();
        await keyCacheService.SetKeyAsync($"tumblr:{ticket}", remoteUser, Duration.FromMinutes(20));

        return Ok(new CallbackResponse(false, ticket, remoteUser.Username, null, null, null));
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterAsync([FromBody] OauthRegisterRequest req)
    {
        RemoteAuthService.RemoteUser? remoteUser =
            await keyCacheService.GetKeyAsync<RemoteAuthService.RemoteUser>($"tumblr:{req.Ticket}");
        if (remoteUser == null)
            throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);
        if (
            await db.AuthMethods.AnyAsync(a =>
                a.AuthType == AuthType.Tumblr && a.RemoteId == remoteUser.Id
            )
        )
        {
            _logger.Error(
                "Tumblr user {Id} has valid ticket but is already linked to an existing account",
                remoteUser.Id
            );
            throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);
        }

        User user = await authService.CreateUserWithRemoteAuthAsync(
            req.Username,
            AuthType.Tumblr,
            remoteUser.Id,
            remoteUser.Username
        );

        return Ok(await authService.GenerateUserTokenAsync(user));
    }

    [HttpGet("add-account")]
    [Authorize("*")]
    public async Task<IActionResult> AddTumblrAccountAsync()
    {
        CheckRequirements();

        string state = await remoteAuthService.ValidateAddAccountRequestAsync(
            CurrentUser!.Id,
            AuthType.Tumblr
        );

        string url =
            "https://www.tumblr.com/oauth2/authorize?response_type=code"
            + $"&client_id={config.TumblrAuth.ClientId}"
            + $"&scope=basic&state={state}"
            + $"&redirect_uri={HttpUtility.UrlEncode($"{config.BaseUrl}/auth/callback/tumblr")}";

        return Ok(new SingleUrlResponse(url));
    }

    [HttpPost("add-account/callback")]
    [Authorize("*")]
    public async Task<IActionResult> AddAccountCallbackAsync([FromBody] CallbackRequest req)
    {
        CheckRequirements();

        await remoteAuthService.ValidateAddAccountStateAsync(
            req.State,
            CurrentUser!.Id,
            AuthType.Tumblr
        );

        RemoteAuthService.RemoteUser remoteUser = await remoteAuthService.RequestTumblrTokenAsync(
            req.Code
        );
        try
        {
            AuthMethod authMethod = await authService.AddAuthMethodAsync(
                CurrentUser.Id,
                AuthType.Tumblr,
                remoteUser.Id,
                remoteUser.Username
            );
            _logger.Debug(
                "Added new Tumblr auth method {AuthMethodId} to user {UserId}",
                authMethod.Id,
                CurrentUser.Id
            );

            return Ok(
                new AddOauthAccountResponse(
                    authMethod.Id,
                    AuthType.Tumblr,
                    authMethod.RemoteId,
                    authMethod.RemoteUsername
                )
            );
        }
        catch (UniqueConstraintException)
        {
            throw new ApiError(
                "That account is already linked.",
                HttpStatusCode.BadRequest,
                ErrorCode.AccountAlreadyLinked
            );
        }
    }

    private void CheckRequirements()
    {
        if (!config.TumblrAuth.Enabled)
        {
            throw new ApiError.BadRequest("Tumblr authentication is not enabled on this instance.");
        }
    }
}
