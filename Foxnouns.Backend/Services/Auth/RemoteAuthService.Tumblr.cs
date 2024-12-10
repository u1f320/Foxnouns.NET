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

// ReSharper disable ClassNeverInstantiated.Local

namespace Foxnouns.Backend.Services.Auth;

public partial class RemoteAuthService
{
    private readonly Uri _tumblrTokenUri = new("https://api.tumblr.com/v2/oauth2/token");
    private readonly Uri _tumblrUserUri = new("https://api.tumblr.com/v2/user/info");

    public async Task<RemoteUser> RequestTumblrTokenAsync(
        string code,
        CancellationToken ct = default
    )
    {
        var redirectUri = $"{config.BaseUrl}/auth/callback/tumblr";
        HttpResponseMessage resp = await _httpClient.PostAsync(
            _tumblrTokenUri,
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "client_id", config.TumblrAuth.ClientId! },
                    { "client_secret", config.TumblrAuth.ClientSecret! },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "scope", "basic" },
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
            throw new FoxnounsError("Invalid Tumblr OAuth response");
        }

        OauthTokenResponse? token = await resp.Content.ReadFromJsonAsync<OauthTokenResponse>(ct);
        if (token == null)
            throw new FoxnounsError("Tumblr token response was null");

        var req = new HttpRequestMessage(HttpMethod.Get, _tumblrUserUri);
        req.Headers.Add("Authorization", $"Bearer {token.AccessToken}");

        HttpResponseMessage resp2 = await _httpClient.SendAsync(req, ct);
        if (!resp2.IsSuccessStatusCode)
        {
            string respBody = await resp2.Content.ReadAsStringAsync(ct);
            _logger.Error(
                "Received error status {StatusCode} when exchanging OAuth token: {ErrorBody}",
                (int)resp2.StatusCode,
                respBody
            );
            throw new FoxnounsError("Invalid Tumblr user response");
        }

        TumblrData? data = await resp2.Content.ReadFromJsonAsync<TumblrData>(ct);
        if (data == null)
            throw new FoxnounsError("Tumblr user response was null");

        TumblrBlog? blog = data.Response.User.Blogs.FirstOrDefault(b => b.Primary);
        if (blog == null)
            throw new FoxnounsError("Tumblr user doesn't have a primary blog");

        return new RemoteUser(blog.Uuid, blog.Name);
    }

    // tumblr why
    private record TumblrData(
        [property: JsonPropertyName("meta")] TumblrMeta Meta,
        [property: JsonPropertyName("response")] TumblrResponse Response
    );

    private record TumblrMeta(
        [property: JsonPropertyName("status")] int Status,
        [property: JsonPropertyName("msg")] string Message
    );

    private record TumblrResponse([property: JsonPropertyName("user")] TumblrUser User);

    private record TumblrUser([property: JsonPropertyName("blogs")] TumblrBlog[] Blogs);

    private record TumblrBlog(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("primary")] bool Primary,
        [property: JsonPropertyName("uuid")] string Uuid
    );
}
