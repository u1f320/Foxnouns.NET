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
using System.Web;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/internal/auth")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthController(
    Config config,
    DatabaseContext db,
    KeyCacheService keyCacheService,
    ILogger logger
) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<AuthController>();

    [HttpPost("urls")]
    [ProducesResponseType<UrlsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UrlsAsync(CancellationToken ct = default)
    {
        _logger.Debug(
            "Generating auth URLs for Discord: {Discord}, Google: {Google}, Tumblr: {Tumblr}",
            config.DiscordAuth.Enabled,
            config.GoogleAuth.Enabled,
            config.TumblrAuth.Enabled
        );
        string state = HttpUtility.UrlEncode(await keyCacheService.GenerateAuthStateAsync(ct));
        string? discord = null;
        string? google = null;
        string? tumblr = null;
        if (config.DiscordAuth is { ClientId: not null, ClientSecret: not null })
        {
            discord =
                "https://discord.com/oauth2/authorize?response_type=code"
                + $"&client_id={config.DiscordAuth.ClientId}&scope=identify"
                + $"&prompt=none&state={state}"
                + $"&redirect_uri={HttpUtility.UrlEncode($"{config.BaseUrl}/auth/callback/discord")}";
        }

        if (config.GoogleAuth is { ClientId: not null, ClientSecret: not null })
        {
            google =
                "https://accounts.google.com/o/oauth2/auth?response_type=code"
                + $"&client_id={config.GoogleAuth.ClientId}"
                + $"&scope=openid+{HttpUtility.UrlEncode("https://www.googleapis.com/auth/userinfo.email")}"
                + $"&prompt=select_account&state={state}"
                + $"&redirect_uri={HttpUtility.UrlEncode($"{config.BaseUrl}/auth/callback/google")}";
        }

        if (config.TumblrAuth is { ClientId: not null, ClientSecret: not null })
        {
            tumblr =
                "https://www.tumblr.com/oauth2/authorize?response_type=code"
                + $"&client_id={config.TumblrAuth.ClientId}"
                + $"&scope=basic&state={state}"
                + $"&redirect_uri={HttpUtility.UrlEncode($"{config.BaseUrl}/auth/callback/tumblr")}";
        }

        return Ok(new UrlsResponse(config.EmailAuth.Enabled, discord, google, tumblr));
    }

    [HttpPost("force-log-out")]
    [Authorize("identify")]
    public async Task<IActionResult> ForceLogoutAsync()
    {
        _logger.Information("Invalidating all tokens for user {UserId}", CurrentUser!.Id);
        await db
            .Tokens.Where(t => t.UserId == CurrentUser.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ManuallyExpired, true));

        return NoContent();
    }

    [HttpGet("methods/{id}")]
    [Authorize("*")]
    [ProducesResponseType<AuthMethodResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthMethodAsync(Snowflake id)
    {
        AuthMethod? authMethod = await db
            .AuthMethods.Include(a => a.FediverseApplication)
            .FirstOrDefaultAsync(a => a.UserId == CurrentUser!.Id && a.Id == id);
        if (authMethod == null)
            throw new ApiError.NotFound("No authentication method with that ID found.");

        return Ok(UserRendererService.RenderAuthMethod(authMethod));
    }

    [HttpDelete("methods/{id}")]
    [Authorize("*")]
    public async Task<IActionResult> DeleteAuthMethodAsync(Snowflake id)
    {
        List<AuthMethod> authMethods = await db
            .AuthMethods.Where(a => a.UserId == CurrentUser!.Id)
            .ToListAsync();
        if (authMethods.Count < 2)
        {
            throw new ApiError(
                "You cannot remove your last authentication method.",
                HttpStatusCode.BadRequest,
                ErrorCode.LastAuthMethod
            );
        }

        AuthMethod? authMethod = authMethods.FirstOrDefault(a => a.Id == id);
        if (authMethod == null)
            throw new ApiError.NotFound("No authentication method with that ID found.");

        _logger.Debug(
            "Deleting auth method {AuthMethodId} for user {UserId}",
            authMethod.Id,
            CurrentUser!.Id
        );

        // If this is the user's last email, we should also clear the user's password.
        if (
            authMethod.AuthType == AuthType.Email
            && authMethods.Count(a => a.AuthType == AuthType.Email) == 1
        )
        {
            _logger.Debug(
                "Deleted last email address for user {UserId}, resetting their password",
                CurrentUser.Id
            );
            CurrentUser.Password = null;
            db.Update(CurrentUser);
        }

        db.Remove(authMethod);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
