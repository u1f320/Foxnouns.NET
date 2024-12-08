using System.Net;
using System.Web;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/internal/auth")]
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
        if (config.DiscordAuth is { ClientId: not null, ClientSecret: not null })
        {
            discord =
                $"https://discord.com/oauth2/authorize?response_type=code"
                + $"&client_id={config.DiscordAuth.ClientId}&scope=identify"
                + $"&prompt=none&state={state}"
                + $"&redirect_uri={HttpUtility.UrlEncode($"{config.BaseUrl}/auth/callback/discord")}";
        }

        return Ok(new UrlsResponse(config.EmailAuth.Enabled, discord, null, null));
    }

    private record UrlsResponse(bool EmailEnabled, string? Discord, string? Google, string? Tumblr);

    public record AuthResponse(
        UserRendererService.UserResponse User,
        string Token,
        Instant ExpiresAt
    );

    public record SingleUrlResponse(string Url);

    public record AddOauthAccountResponse(
        Snowflake Id,
        [property: JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))] AuthType Type,
        string RemoteId,
        [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            string? RemoteUsername
    );

    public record OauthRegisterRequest(string Ticket, string Username);

    public record CallbackRequest(string Code, string State);

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
    [ProducesResponseType<UserRendererService.AuthMethodResponse>(
        statusCode: StatusCodes.Status200OK
    )]
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

public record CallbackResponse(
    bool HasAccount,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? Ticket,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? RemoteUsername,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        UserRendererService.UserResponse? User,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? Token,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] Instant? ExpiresAt
);
