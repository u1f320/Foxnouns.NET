using System.Web;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/v2/auth")]
public class AuthController(Config config, KeyCacheService keyCacheSvc, ILogger logger) : ApiControllerBase
{
    [HttpPost("urls")]
    [ProducesResponseType<UrlsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UrlsAsync()
    {
        logger.Debug("Generating auth URLs for Discord: {Discord}, Google: {Google}, Tumblr: {Tumblr}",
            config.DiscordAuth.Enabled,
            config.GoogleAuth.Enabled,
            config.TumblrAuth.Enabled);
        var state = HttpUtility.UrlEncode(await keyCacheSvc.GenerateAuthStateAsync());
        string? discord = null;
        if (config.DiscordAuth is { ClientId: not null, ClientSecret: not null })
            discord =
                $"https://discord.com/oauth2/authorize?response_type=code" +
                $"&client_id={config.DiscordAuth.ClientId}&scope=identify" +
                $"&prompt=none&state={state}" +
                $"&redirect_uri={HttpUtility.UrlEncode($"{config.BaseUrl}/auth/login/discord")}";

        return Ok(new UrlsResponse(discord, null, null));
    }

    private record UrlsResponse(
        string? Discord,
        string? Google,
        string? Tumblr
    );

    public record AuthResponse(
        UserRendererService.UserResponse User,
        string Token,
        Instant ExpiresAt
    );

    public record CallbackResponse(
        bool HasAccount, // If true, user has an account, but it's deleted
        string Ticket,
        string? RemoteUsername
    );

    public record OauthRegisterRequest(string Ticket, string Username);

    public record CallbackRequest(string Code, string State);
}