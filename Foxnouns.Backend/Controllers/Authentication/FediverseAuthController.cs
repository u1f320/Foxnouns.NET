// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.Net;
using EntityFramework.Exceptions.Common;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Services.Auth;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/internal/auth/fediverse")]
[ApiExplorerSettings(IgnoreApi = true)]
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
    [ProducesResponseType<SingleUrlResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFediverseUrlAsync(
        [FromQuery] string instance,
        [FromQuery] bool forceRefresh = false
    )
    {
        if (instance.Any(c => c is '@' or ':' or '/') || !instance.Contains('.'))
            throw new ApiError.BadRequest("Not a valid domain.", "instance", instance);

        string url = await fediverseAuthService.GenerateAuthUrlAsync(instance, forceRefresh);
        return Ok(new SingleUrlResponse(url));
    }

    [HttpPost("callback")]
    [ProducesResponseType<CallbackResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> FediverseCallbackAsync([FromBody] FediverseCallbackRequest req)
    {
        FediverseApplication app = await fediverseAuthService.GetApplicationAsync(req.Instance);
        FediverseAuthService.FediverseUser remoteUser =
            await fediverseAuthService.GetRemoteFediverseUserAsync(app, req.Code, req.State);

        User? user = await authService.AuthenticateUserAsync(
            AuthType.Fediverse,
            remoteUser.Id,
            app
        );
        if (user != null)
            return Ok(await authService.GenerateUserTokenAsync(user));

        string ticket = AuthUtils.RandomToken();
        await keyCacheService.SetKeyAsync(
            $"fediverse:{ticket}",
            new FediverseTicketData(app.Id, remoteUser),
            Duration.FromMinutes(20)
        );

        return Ok(
            new CallbackResponse(
                false,
                ticket,
                $"@{remoteUser.Username}@{app.Domain}",
                null,
                null,
                null
            )
        );
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterAsync([FromBody] OauthRegisterRequest req)
    {
        FediverseTicketData? ticketData = await keyCacheService.GetKeyAsync<FediverseTicketData>(
            $"fediverse:{req.Ticket}",
            true
        );
        if (ticketData == null)
            throw new ApiError.BadRequest("Invalid ticket", "ticket", req.Ticket);

        FediverseApplication? app = await db.FediverseApplications.FindAsync(
            ticketData.ApplicationId
        );
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

        User user = await authService.CreateUserWithRemoteAuthAsync(
            req.Username,
            AuthType.Fediverse,
            ticketData.User.Id,
            ticketData.User.Username,
            app
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

        string state = await remoteAuthService.ValidateAddAccountRequestAsync(
            CurrentUser!.Id,
            AuthType.Fediverse,
            instance
        );

        string url = await fediverseAuthService.GenerateAuthUrlAsync(instance, forceRefresh, state);
        return Ok(new SingleUrlResponse(url));
    }

    [HttpPost("add-account/callback")]
    [Authorize("*")]
    public async Task<IActionResult> AddAccountCallbackAsync(
        [FromBody] FediverseCallbackRequest req
    )
    {
        FediverseApplication app = await fediverseAuthService.GetApplicationAsync(req.Instance);
        FediverseAuthService.FediverseUser remoteUser =
            await fediverseAuthService.GetRemoteFediverseUserAsync(app, req.Code);
        try
        {
            AuthMethod authMethod = await authService.AddAuthMethodAsync(
                CurrentUser!.Id,
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
                new AddOauthAccountResponse(
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

    private record FediverseTicketData(
        Snowflake ApplicationId,
        FediverseAuthService.FediverseUser User
    );
}
