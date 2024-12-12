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
using System.Text.Json.Serialization;
using System.Web;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;

namespace Foxnouns.Backend.Services.Auth;

public partial class FediverseAuthService
{
    private static string MisskeyAppUri(string instance) => $"https://{instance}/api/app/create";

    private static string MisskeyTokenUri(string instance) =>
        $"https://{instance}/api/auth/session/userkey";

    private static string MisskeyGenerateSessionUri(string instance) =>
        $"https://{instance}/api/auth/session/generate";

    private async Task<FediverseApplication> CreateMisskeyApplicationAsync(
        string instance,
        Snowflake? existingAppId = null
    )
    {
        HttpResponseMessage resp = await _client.PostAsJsonAsync(
            MisskeyAppUri(instance),
            new CreateMisskeyApplicationRequest(
                $"pronouns.cc (+{_config.BaseUrl})",
                $"pronouns.cc on {_config.BaseUrl}",
                ["read:account"],
                MastodonRedirectUri(instance)
            )
        );
        resp.EnsureSuccessStatusCode();

        PartialMisskeyApplication? misskeyApp =
            await resp.Content.ReadFromJsonAsync<PartialMisskeyApplication>();
        if (misskeyApp == null)
        {
            throw new FoxnounsError(
                $"Application created on Misskey-compatible instance {instance} was null"
            );
        }

        FediverseApplication app;

        if (existingAppId == null)
        {
            app = new FediverseApplication
            {
                Id = existingAppId ?? _snowflakeGenerator.GenerateSnowflake(),
                ClientId = misskeyApp.Id,
                ClientSecret = misskeyApp.Secret,
                Domain = instance,
                InstanceType = FediverseInstanceType.MisskeyApi,
            };

            _db.Add(app);
        }
        else
        {
            app =
                await _db.FediverseApplications.FindAsync(existingAppId)
                ?? throw new FoxnounsError($"Existing app with ID {existingAppId} was null");

            app.ClientId = misskeyApp.Id;
            app.ClientSecret = misskeyApp.Secret;
            app.InstanceType = FediverseInstanceType.MisskeyApi;
        }

        await _db.SaveChangesAsync();

        return app;
    }

    private record GetMisskeySessionUserKeyRequest(
        [property: JsonPropertyName("appSecret")] string Secret,
        [property: JsonPropertyName("token")] string Token
    );

    private record GetMisskeySessionUserKeyResponse(
        [property: JsonPropertyName("user")] FediverseUser User
    );

    private async Task<FediverseUser> GetMisskeyUserAsync(FediverseApplication app, string code)
    {
        HttpResponseMessage resp = await _client.PostAsJsonAsync(
            MisskeyTokenUri(app.Domain),
            new GetMisskeySessionUserKeyRequest(app.ClientSecret, code)
        );
        if (resp.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new FoxnounsError($"Application for instance {app.Domain} was invalid");
        }

        resp.EnsureSuccessStatusCode();
        GetMisskeySessionUserKeyResponse? userResp =
            await resp.Content.ReadFromJsonAsync<GetMisskeySessionUserKeyResponse>();
        if (userResp == null)
        {
            throw new FoxnounsError($"User response from instance {app.Domain} was invalid");
        }

        return userResp.User;
    }

    private async Task<string> GenerateMisskeyAuthUrlAsync(
        FediverseApplication app,
        bool forceRefresh,
        string? state = null
    )
    {
        if (forceRefresh)
        {
            _logger.Information(
                "An app credentials refresh was requested for {ApplicationId}, creating a new application",
                app.Id
            );
            app = await CreateMisskeyApplicationAsync(app.Domain, app.Id);
        }

        HttpResponseMessage resp = await _client.PostAsJsonAsync(
            MisskeyGenerateSessionUri(app.Domain),
            new CreateMisskeySessionUriRequest(app.ClientSecret)
        );
        resp.EnsureSuccessStatusCode();

        CreateMisskeySessionUriResponse? misskeyResp =
            await resp.Content.ReadFromJsonAsync<CreateMisskeySessionUriResponse>();
        if (misskeyResp == null)
            throw new FoxnounsError($"Session create response for app {app.Id} was null");

        return misskeyResp.Url;
    }

    private record CreateMisskeySessionUriRequest(
        [property: JsonPropertyName("appSecret")] string Secret
    );

    private record CreateMisskeySessionUriResponse(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("url")] string Url
    );

    private record CreateMisskeyApplicationRequest(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("permission")] string[] Permissions,
        [property: JsonPropertyName("callbackUrl")] string CallbackUrl
    );

    private record PartialMisskeyApplication(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("secret")] string Secret
    );
}
