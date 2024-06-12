using System.Web;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/v2/auth")]
public class AuthController(Config config, KeyCacheService keyCacheSvc) : ApiControllerBase
{   
    [HttpPost("urls")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UrlsResponse))]
    public async Task<IActionResult> UrlsAsync()
    {
        var state = HttpUtility.UrlEncode(await keyCacheSvc.GenerateAuthStateAsync());
        string? discord = null;
        if (config.DiscordAuth.ClientId != null && config.DiscordAuth.ClientSecret != null)
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

    internal record AuthResponse(
        UserRendererService.UserResponse User,
        string Token,
        Instant ExpiresAt
    );
}