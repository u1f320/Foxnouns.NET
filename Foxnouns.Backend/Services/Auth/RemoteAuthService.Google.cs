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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foxnouns.Backend.Services.Auth;

public partial class RemoteAuthService
{
    private readonly Uri _googleTokenUri = new("https://oauth2.googleapis.com/token");

    public async Task<RemoteUser> RequestGoogleTokenAsync(
        string code,
        CancellationToken ct = default
    )
    {
        var redirectUri = $"{config.BaseUrl}/auth/callback/google";
        HttpResponseMessage resp = await _httpClient.PostAsync(
            _googleTokenUri,
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "client_id", config.GoogleAuth.ClientId! },
                    { "client_secret", config.GoogleAuth.ClientSecret! },
                    { "grant_type", "authorization_code" },
                    { "scope", "openid https://www.googleapis.com/auth/userinfo.email" },
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
            throw new FoxnounsError("Invalid Google OAuth response");
        }

        GoogleTokenResponse? token = await resp.Content.ReadFromJsonAsync<GoogleTokenResponse>(ct);
        if (token == null)
            throw new FoxnounsError("Google token response was null");

        byte[] rawIdToken = Convert.FromBase64String(token.IdToken.Split(".")[1]);
        GoogleUser? user = JsonSerializer.Deserialize<GoogleUser>(
            Encoding.UTF8.GetString(rawIdToken)
        );
        if (user == null)
            throw new FoxnounsError("Google user was null");

        return new RemoteUser(user.Id, user.Email);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record GoogleTokenResponse([property: JsonPropertyName("id_token")] string IdToken);

    private record GoogleUser(
        [property: JsonPropertyName("sub")] string Id,
        [property: JsonPropertyName("email")] string Email
    );
}
