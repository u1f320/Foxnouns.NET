using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using J = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace Foxnouns.Backend.Services.Auth;

public partial class FediverseAuthService
{
    private string MastodonRedirectUri(string instance) =>
        $"{_config.BaseUrl}/auth/callback/mastodon/{instance}";

    private async Task<FediverseApplication> CreateMastodonApplicationAsync(
        string instance,
        Snowflake? existingAppId = null
    )
    {
        HttpResponseMessage resp = await _client.PostAsJsonAsync(
            $"https://{instance}/api/v1/apps",
            new CreateMastodonApplicationRequest(
                $"pronouns.cc (+{_config.BaseUrl})",
                MastodonRedirectUri(instance),
                "read read:accounts",
                _config.BaseUrl
            )
        );
        resp.EnsureSuccessStatusCode();

        PartialMastodonApplication? mastodonApp =
            await resp.Content.ReadFromJsonAsync<PartialMastodonApplication>();
        if (mastodonApp == null)
        {
            throw new FoxnounsError(
                $"Application created on Mastodon-compatible instance {instance} was null"
            );
        }

        FediverseApplication app;

        if (existingAppId == null)
        {
            app = new FediverseApplication
            {
                Id = existingAppId ?? _snowflakeGenerator.GenerateSnowflake(),
                ClientId = mastodonApp.ClientId,
                ClientSecret = mastodonApp.ClientSecret,
                Domain = instance,
                InstanceType = FediverseInstanceType.MastodonApi,
            };

            _db.Add(app);
        }
        else
        {
            app =
                await _db.FediverseApplications.FindAsync(existingAppId)
                ?? throw new FoxnounsError($"Existing app with ID {existingAppId} was null");

            app.ClientId = mastodonApp.ClientId;
            app.ClientSecret = mastodonApp.ClientSecret;
            app.InstanceType = FediverseInstanceType.MastodonApi;
        }

        await _db.SaveChangesAsync();

        return app;
    }

    private async Task<FediverseUser> GetMastodonUserAsync(
        FediverseApplication app,
        string code,
        string? state = null
    )
    {
        if (state != null)
            await _keyCacheService.ValidateAuthStateAsync(state);

        HttpResponseMessage tokenResp = await _client.PostAsync(
            MastodonTokenUri(app.Domain),
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "scope", "read:accounts" },
                    { "client_id", app.ClientId },
                    { "client_secret", app.ClientSecret },
                    { "redirect_uri", MastodonRedirectUri(app.Domain) },
                }
            )
        );
        if (tokenResp.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new FoxnounsError($"Application for instance {app.Domain} was invalid");
        }

        tokenResp.EnsureSuccessStatusCode();
        string? token = (
            await tokenResp.Content.ReadFromJsonAsync<MastodonTokenResponse>()
        )?.AccessToken;
        if (token == null)
        {
            throw new FoxnounsError($"Token response from instance {app.Domain} was invalid");
        }

        var req = new HttpRequestMessage(HttpMethod.Get, MastodonCurrentUserUri(app.Domain));
        req.Headers.Add("Authorization", $"Bearer {token}");

        HttpResponseMessage currentUserResp = await _client.SendAsync(req);
        currentUserResp.EnsureSuccessStatusCode();
        FediverseUser? user = await currentUserResp.Content.ReadFromJsonAsync<FediverseUser>();
        if (user == null)
        {
            throw new FoxnounsError($"User response from instance {app.Domain} was invalid");
        }

        return user;
    }

    private record MastodonTokenResponse([property: J("access_token")] string AccessToken);

    private async Task<string> GenerateMastodonAuthUrlAsync(
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
            app = await CreateMastodonApplicationAsync(app.Domain, app.Id);
        }

        state ??= HttpUtility.UrlEncode(await _keyCacheService.GenerateAuthStateAsync());

        return $"https://{app.Domain}/oauth/authorize?response_type=code"
            + $"&client_id={app.ClientId}"
            + $"&scope={HttpUtility.UrlEncode("read:accounts")}"
            + $"&redirect_uri={HttpUtility.UrlEncode(MastodonRedirectUri(app.Domain))}"
            + $"&state={state}";
    }

    private static string MastodonTokenUri(string instance) => $"https://{instance}/oauth/token";

    private static string MastodonCurrentUserUri(string instance) =>
        $"https://{instance}/api/v1/accounts/verify_credentials";

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private record PartialMastodonApplication(
        [property: J("name")] string Name,
        [property: J("client_id")] string ClientId,
        [property: J("client_secret")] string ClientSecret
    );

    private record CreateMastodonApplicationRequest(
        [property: J("client_name")] string ClientName,
        [property: J("redirect_uris")] string RedirectUris,
        [property: J("scopes")] string Scopes,
        [property: J("website")] string Website
    );
}
