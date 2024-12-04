using System.Net;
using EntityFramework.Exceptions.Common;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Services.Auth;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/internal/auth/fediverse")]
public class FediverseAuthController(
    ILogger logger,
    DatabaseContext db,
    FediverseAuthService fediverseAuthService,
    AuthService authService,
    RemoteAuthService remoteAuthService,
    KeyCacheService keyCacheService
) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<FediverseAuthController>();

    [HttpGet]
    [ProducesResponseType<AuthController.SingleUrlResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFediverseUrlAsync(
        [FromQuery] string instance,
        [FromQuery] bool forceRefresh = false
    )
    {
        if (instance.Any(c => c is '@' or ':' or '/') || !instance.Contains('.'))
            throw new ApiError.BadRequest("Not a valid domain.", "instance", instance);

        var url = await fediverseAuthService.GenerateAuthUrlAsync(instance, forceRefresh);
        return Ok(new AuthController.SingleUrlResponse(url));
    }

    [HttpPost("callback")]
    [ProducesResponseType<CallbackResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> FediverseCallbackAsync([FromBody] CallbackRequest req)
    {
        var app = await fediverseAuthService.GetApplicationAsync(req.Instance);
        var remoteUser = await fediverseAuthService.GetRemoteFediverseUserAsync(
            app,
            req.Code,
            req.State
        );

        var user = await authService.AuthenticateUserAsync(
            AuthType.Fediverse,
            remoteUser.Id,
            instance: app
        );
        if (user != null)
            return Ok(await authService.GenerateUserTokenAsync(user));

        var ticket = AuthUtils.RandomToken();
        await keyCacheService.SetKeyAsync(
            $"fediverse:{ticket}",
            new FediverseTicketData(app.Id, remoteUser),
            Duration.FromMinutes(20)
        );

        return Ok(
            new CallbackResponse(
                HasAccount: false,
                Ticket: ticket,
                RemoteUsername: $"@{remoteUser.Username}@{app.Domain}",
                User: null,
                Token: null,
                ExpiresAt: null
            )
        );
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthController.AuthResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] AuthController.OauthRegisterRequest req
    )
    {
        var ticketData = await keyCacheService.GetKeyAsync<FediverseTicketData>(
            $"fediverse:{req.Ticket}",
            delete: true
        );
        if (ticketData == null)
            throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);

        var app = await db.FediverseApplications.FindAsync(ticketData.ApplicationId);
        if (app == null)
            throw new FoxnounsError("Null application found for ticket");

        if (
            await db.AuthMethods.AnyAsync(a =>
                a.AuthType == AuthType.Fediverse
                && a.RemoteId == ticketData.User.Id
                && a.FediverseApplicationId == app.Id
            )
        )
        {
            _logger.Error(
                "Fediverse user {Id}/{ApplicationId} ({Username} on {Domain}) has valid ticket but is already linked to an existing account",
                ticketData.User.Id,
                ticketData.ApplicationId,
                ticketData.User.Username,
                app.Domain
            );
            throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);
        }

        var user = await authService.CreateUserWithRemoteAuthAsync(
            req.Username,
            AuthType.Fediverse,
            ticketData.User.Id,
            ticketData.User.Username,
            instance: app
        );

        return Ok(await authService.GenerateUserTokenAsync(user));
    }

    [HttpGet("add-account")]
    [Authorize("*")]
    public async Task<IActionResult> AddFediverseAccountAsync(
        [FromQuery] string instance,
        [FromQuery] bool forceRefresh = false
    )
    {
        if (instance.Any(c => c is '@' or ':' or '/') || !instance.Contains('.'))
            throw new ApiError.BadRequest("Not a valid domain.", "instance", instance);

        var state = await remoteAuthService.ValidateAddAccountRequestAsync(
            CurrentUser!.Id,
            AuthType.Fediverse,
            instance
        );

        var url = await fediverseAuthService.GenerateAuthUrlAsync(instance, forceRefresh, state);
        return Ok(new AuthController.SingleUrlResponse(url));
    }

    [HttpPost("add-account/callback")]
    [Authorize("*")]
    public async Task<IActionResult> AddAccountCallbackAsync([FromBody] CallbackRequest req)
    {
        await remoteAuthService.ValidateAddAccountStateAsync(
            req.State,
            CurrentUser!.Id,
            AuthType.Fediverse,
            req.Instance
        );

        var app = await fediverseAuthService.GetApplicationAsync(req.Instance);
        var remoteUser = await fediverseAuthService.GetRemoteFediverseUserAsync(app, req.Code);
        try
        {
            var authMethod = await authService.AddAuthMethodAsync(
                CurrentUser.Id,
                AuthType.Fediverse,
                remoteUser.Id,
                remoteUser.Username,
                app
            );
            _logger.Debug(
                "Added new Fediverse auth method {AuthMethodId} to user {UserId}",
                authMethod.Id,
                CurrentUser.Id
            );

            return Ok(
                new AuthController.AddOauthAccountResponse(
                    authMethod.Id,
                    AuthType.Fediverse,
                    authMethod.RemoteId,
                    $"{authMethod.RemoteUsername}@{app.Domain}"
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

    public record CallbackRequest(string Instance, string Code, string State);

    private record FediverseTicketData(
        Snowflake ApplicationId,
        FediverseAuthService.FediverseUser User
    );
}
