using System.Net;
using System.Web;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Duration = NodaTime.Duration;
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
        var resp = await _client.PostAsync(
            $"https://{instance}/api/v1/apps",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "client_name", $"pronouns.cc (+{_config.BaseUrl})" },
                    { "redirect_uris", MastodonRedirectUri(instance) },
                    { "scope", "read:accounts" },
                    { "website", _config.BaseUrl },
                }
            )
        );
        resp.EnsureSuccessStatusCode();

        var mastodonApp = await resp.Content.ReadFromJsonAsync<PartialMastodonApplication>();
        if (mastodonApp == null)
            throw new FoxnounsError(
                $"Application created on Mastodon-compatible instance {instance} was null"
            );

        var token = await GetMastodonAppTokenAsync(
            instance,
            mastodonApp.ClientId,
            mastodonApp.ClientSecret
        );

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
                AccessToken = token,
                TokenValidUntil = _clock.GetCurrentInstant() + Duration.FromDays(60),
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
            app.AccessToken = null;
            app.TokenValidUntil = null;
        }

        await _db.SaveChangesAsync();

        return app;
    }

    private async Task<FediverseUser> GetMastodonUserAsync(FediverseApplication app, string code)
    {
        var tokenResp = await _client.PostAsync(
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
        var token = (
            await tokenResp.Content.ReadFromJsonAsync<MastodonTokenResponse>()
        )?.AccessToken;
        if (token == null)
        {
            throw new FoxnounsError($"Token response from instance {app.Domain} was invalid");
        }

        var req = new HttpRequestMessage(HttpMethod.Get, MastodonCurrentUserUri(app.Domain));
        req.Headers.Add("Authorization", $"Bearer {token}");

        var currentUserResp = await _client.SendAsync(req);
        currentUserResp.EnsureSuccessStatusCode();
        var user = await currentUserResp.Content.ReadFromJsonAsync<FediverseUser>();
        if (user == null)
        {
            throw new FoxnounsError($"User response from instance {app.Domain} was invalid");
        }

        return user;
    }

    private record MastodonTokenResponse([property: J("access_token")] string AccessToken);

    // TODO: Mastodon's OAuth documentation doesn't specify a "state" parameter. that feels... wrong
    // https://docs.joinmastodon.org/methods/oauth/
    private async Task<string> GenerateMastodonAuthUrlAsync(FediverseApplication app)
    {
        try
        {
            await ValidateMastodonAppAsync(app);
        }
        catch (FoxnounsError.RemoteAuthError e)
        {
            _logger.Error(
                e,
                "Error validating app token for {AppId} on {Instance}",
                app.Id,
                app.Domain
            );

            app = await CreateMastodonApplicationAsync(app.Domain, existingAppId: app.Id);
        }

        return $"https://{app.Domain}/oauth/authorize?response_type=code"
            + $"&client_id={app.ClientId}"
            + $"&scope={HttpUtility.UrlEncode("read:accounts")}"
            + $"&redirect_uri={HttpUtility.UrlEncode(MastodonRedirectUri(app.Domain))}";
    }

    private async Task ValidateMastodonAppAsync(FediverseApplication app)
    {
        // If we don't have an access token stored, or it's too old, get one
        // When doing this we don't need to fetch the application info
        if (app.AccessToken == null || app.TokenValidUntil < _clock.GetCurrentInstant())
        {
            _logger.Debug(
                "Application {AppId} on instance {Instance} has no valid token, fetching it",
                app.Id,
                app.Domain
            );

            app.AccessToken = await GetMastodonAppTokenAsync(
                app.Domain,
                app.ClientId,
                app.ClientSecret
            );
            app.TokenValidUntil = _clock.GetCurrentInstant() + Duration.FromDays(60);

            _db.Update(app);
            await _db.SaveChangesAsync();
            return;
        }

        _logger.Debug(
            "Checking whether application {AppId} on instance {Instance} is still valid",
            app.Id,
            app.Domain
        );

        var req = new HttpRequestMessage(HttpMethod.Get, MastodonCurrentAppUri(app.Domain));
        req.Headers.Add("Authorization", $"Bearer {app.AccessToken}");

        var resp = await _client.SendAsync(req);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new FoxnounsError.RemoteAuthError(
                "Verifying app credentials returned an error",
                error
            );
        }
    }

    private async Task<string> GetMastodonAppTokenAsync(
        string instance,
        string clientId,
        string clientSecret
    )
    {
        var resp = await _client.PostAsync(
            MastodonTokenUri(instance),
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                }
            )
        );
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new FoxnounsError.RemoteAuthError(
                "Requesting app token returned an error",
                error
            );
        }

        var token = (await resp.Content.ReadFromJsonAsync<MastodonTokenResponse>())?.AccessToken;
        if (token == null)
        {
            throw new FoxnounsError($"Token response from instance {instance} was invalid");
        }

        return token;
    }

    private static string MastodonTokenUri(string instance) => $"https://{instance}/oauth/token";

    private static string MastodonCurrentUserUri(string instance) =>
        $"https://{instance}/api/v1/accounts/verify_credentials";

    private static string MastodonCurrentAppUri(string instance) =>
        $"https://{instance}/api/v1/apps/verify_credentials";

    private record PartialMastodonApplication(
        [property: J("name")] string Name,
        [property: J("client_id")] string ClientId,
        [property: J("client_secret")] string ClientSecret
    );
}
