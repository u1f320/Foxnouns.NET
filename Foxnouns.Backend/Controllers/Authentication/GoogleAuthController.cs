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

[Route("/api/internal/auth/google")]
public class GoogleAuthController(
    [UsedImplicitly] Config config,
    ILogger logger,
    DatabaseContext db,
    KeyCacheService keyCacheService,
    AuthService authService,
    RemoteAuthService remoteAuthService
) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<GoogleAuthController>();

    [HttpPost("callback")]
    [ProducesResponseType<CallbackResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CallbackAsync([FromBody] CallbackRequest req)
    {
        CheckRequirements();
        await keyCacheService.ValidateAuthStateAsync(req.State);

        RemoteAuthService.RemoteUser remoteUser = await remoteAuthService.RequestGoogleTokenAsync(
            req.Code
        );
        User? user = await authService.AuthenticateUserAsync(AuthType.Google, remoteUser.Id);
        if (user != null)
            return Ok(await authService.GenerateUserTokenAsync(user));

        _logger.Debug(
            "Google user {Username} ({Id}) authenticated with no local account",
            remoteUser.Username,
            remoteUser.Id
        );

        string ticket = AuthUtils.RandomToken();
        await keyCacheService.SetKeyAsync($"google:{ticket}", remoteUser, Duration.FromMinutes(20));

        return Ok(new CallbackResponse(false, ticket, remoteUser.Username, null, null, null));
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterAsync([FromBody] OauthRegisterRequest req)
    {
        RemoteAuthService.RemoteUser? remoteUser =
            await keyCacheService.GetKeyAsync<RemoteAuthService.RemoteUser>($"google:{req.Ticket}");
        if (remoteUser == null)
            throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);
        if (
            await db.AuthMethods.AnyAsync(a =>
                a.AuthType == AuthType.Google && a.RemoteId == remoteUser.Id
            )
        )
        {
            _logger.Error(
                "Google user {Id} has valid ticket but is already linked to an existing account",
                remoteUser.Id
            );
            throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);
        }

        User user = await authService.CreateUserWithRemoteAuthAsync(
            req.Username,
            AuthType.Google,
            remoteUser.Id,
            remoteUser.Username
        );

        return Ok(await authService.GenerateUserTokenAsync(user));
    }

    [HttpGet("add-account")]
    [Authorize("*")]
    public async Task<IActionResult> AddGoogleAccountAsync()
    {
        CheckRequirements();

        string state = await remoteAuthService.ValidateAddAccountRequestAsync(
            CurrentUser!.Id,
            AuthType.Google
        );

        string url =
            "https://accounts.google.com/o/oauth2/auth?response_type=code"
            + $"&client_id={config.GoogleAuth.ClientId}"
            + $"&scope=openid+{HttpUtility.UrlEncode("https://www.googleapis.com/auth/userinfo.email")}"
            + $"&prompt=select_account&state={state}"
            + $"&redirect_uri={HttpUtility.UrlEncode($"{config.BaseUrl}/auth/callback/google")}";

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
            AuthType.Google
        );

        RemoteAuthService.RemoteUser remoteUser = await remoteAuthService.RequestGoogleTokenAsync(
            req.Code
        );
        try
        {
            AuthMethod authMethod = await authService.AddAuthMethodAsync(
                CurrentUser.Id,
                AuthType.Google,
                remoteUser.Id,
                remoteUser.Username
            );
            _logger.Debug(
                "Added new Google auth method {AuthMethodId} to user {UserId}",
                authMethod.Id,
                CurrentUser.Id
            );

            return Ok(
                new AddOauthAccountResponse(
                    authMethod.Id,
                    AuthType.Google,
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
        if (!config.GoogleAuth.Enabled)
        {
            throw new ApiError.BadRequest("Google authentication is not enabled on this instance.");
        }
    }
}
