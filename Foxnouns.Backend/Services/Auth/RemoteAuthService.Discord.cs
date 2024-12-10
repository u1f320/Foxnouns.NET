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
using System.Text.Json.Serialization;

namespace Foxnouns.Backend.Services.Auth;

public partial class RemoteAuthService
{
    private readonly Uri _discordTokenUri = new("https://discord.com/api/oauth2/token");
    private readonly Uri _discordUserUri = new("https://discord.com/api/v10/users/@me");

    public async Task<RemoteUser> RequestDiscordTokenAsync(
        string code,
        CancellationToken ct = default
    )
    {
        var redirectUri = $"{config.BaseUrl}/auth/callback/discord";
        HttpResponseMessage resp = await _httpClient.PostAsync(
            _discordTokenUri,
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "client_id", config.DiscordAuth.ClientId! },
                    { "client_secret", config.DiscordAuth.ClientSecret! },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri },
                }
            ),
            ct
        );
        if (!resp.IsSuccessStatusCode)
        {
            string respBody = await resp.Content.ReadAsStringAsync(ct);
            _logger.Error(
                "Received error status {StatusCode} when exchanging OAuth token: {ErrorBody}",
                (int)resp.StatusCode,
                respBody
            );
            throw new FoxnounsError("Invalid Discord OAuth response");
        }

        OauthTokenResponse? token = await resp.Content.ReadFromJsonAsync<OauthTokenResponse>(ct);
        if (token == null)
            throw new FoxnounsError("Discord token response was null");

        var req = new HttpRequestMessage(HttpMethod.Get, _discordUserUri);
        req.Headers.Add("Authorization", $"{token.TokenType} {token.AccessToken}");

        HttpResponseMessage resp2 = await _httpClient.SendAsync(req, ct);
        resp2.EnsureSuccessStatusCode();
        DiscordUserResponse? user = await resp2.Content.ReadFromJsonAsync<DiscordUserResponse>(ct);
        if (user == null)
            throw new FoxnounsError("Discord user response was null");

        return new RemoteUser(user.Id, user.Username);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record DiscordUserResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("username")] string Username
    );
}
