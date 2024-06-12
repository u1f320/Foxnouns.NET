using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Foxnouns.Backend.Services;

public class RemoteAuthService(Config config)
{
    private readonly HttpClient _httpClient = new();

    private readonly Uri _discordTokenUri = new("https://discord.com/api/oauth2/token");
    private readonly Uri _discordUserUri = new("https://discord.com/api/v10/users/@me");

    public async Task<RemoteUser> RequestDiscordTokenAsync(string code, string state)
    {
        var redirectUri = $"{config.BaseUrl}/auth/login/discord";
        var resp = await _httpClient.PostAsync(_discordTokenUri, new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                { "client_id", config.DiscordAuth.ClientId! },
                { "client_secret", config.DiscordAuth.ClientSecret! },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri }
            }
        ));
        resp.EnsureSuccessStatusCode();
        var token = await resp.Content.ReadFromJsonAsync<DiscordTokenResponse>();
        if (token == null) throw new FoxnounsError("Discord token response was null");

        var req = new HttpRequestMessage(HttpMethod.Get, _discordUserUri);
        req.Headers.Add("Authorization", $"{token.token_type} {token.access_token}");

        var resp2 = await _httpClient.SendAsync(req);
        resp2.EnsureSuccessStatusCode();
        var user = await resp2.Content.ReadFromJsonAsync<DiscordUserResponse>();
        if (user == null) throw new FoxnounsError("Discord user response was null");

        return new RemoteUser(user.id, user.username);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private record DiscordTokenResponse(string access_token, string token_type);

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private record DiscordUserResponse(string id, string username);

    public record RemoteUser(string Id, string Username);
}