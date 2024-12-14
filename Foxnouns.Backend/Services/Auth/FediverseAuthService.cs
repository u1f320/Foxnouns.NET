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
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using J = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace Foxnouns.Backend.Services.Auth;

public partial class FediverseAuthService
{
    private const string NodeInfoRel = "http://nodeinfo.diaspora.software/ns/schema/2.0";

    private readonly HttpClient _client;
    private readonly ILogger _logger;
    private readonly Config _config;
    private readonly DatabaseContext _db;
    private readonly KeyCacheService _keyCacheService;
    private readonly ISnowflakeGenerator _snowflakeGenerator;

    public FediverseAuthService(
        ILogger logger,
        Config config,
        DatabaseContext db,
        KeyCacheService keyCacheService,
        ISnowflakeGenerator snowflakeGenerator
    )
    {
        _logger = logger.ForContext<FediverseAuthService>();
        _config = config;
        _db = db;
        _keyCacheService = keyCacheService;
        _snowflakeGenerator = snowflakeGenerator;

        _client = new HttpClient();
        _client.DefaultRequestHeaders.Remove("User-Agent");
        _client.DefaultRequestHeaders.Remove("Accept");
        _client.DefaultRequestHeaders.Add("User-Agent", $"pronouns.cc/{BuildInfo.Version}");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<string> GenerateAuthUrlAsync(
        string instance,
        bool forceRefresh,
        string? state = null
    )
    {
        FediverseApplication app = await GetApplicationAsync(instance);
        return await GenerateAuthUrlAsync(app, forceRefresh, state);
    }

    // thank you, gargron and syuilo, for agreeing on a name for *once* in your lives,
    // and having both mastodon and misskey use "username" in the self user response
    public record FediverseUser(
        [property: J("id")] string Id,
        [property: J("username")] string Username
    );

    public async Task<FediverseApplication> GetApplicationAsync(string instance)
    {
        FediverseApplication? app = await _db.FediverseApplications.FirstOrDefaultAsync(a =>
            a.Domain == instance
        );
        if (app != null)
            return app;

        _logger.Debug("No application for fediverse instance {Instance}, creating it", instance);

        string softwareName = await GetSoftwareNameAsync(instance);

        if (IsMastodonCompatible(softwareName))
            return await CreateMastodonApplicationAsync(instance);
        if (IsMisskeyCompatible(softwareName))
            return await CreateMisskeyApplicationAsync(instance);

        throw new ApiError.BadRequest($"{softwareName} is not a supported instance type, sorry.");
    }

    private async Task<string> GetSoftwareNameAsync(string instance)
    {
        _logger.Debug("Requesting software name for fediverse instance {Instance}", instance);

        HttpResponseMessage wellKnownResp = await _client.GetAsync(
            new Uri($"https://{instance}/.well-known/nodeinfo")
        );
        wellKnownResp.EnsureSuccessStatusCode();

        WellKnownResponse? wellKnown =
            await wellKnownResp.Content.ReadFromJsonAsync<WellKnownResponse>();
        string? nodeInfoUrl = wellKnown?.Links.FirstOrDefault(l => l.Rel == NodeInfoRel)?.Href;
        if (nodeInfoUrl == null)
        {
            throw new FoxnounsError(
                $".well-known/nodeinfo response for instance {instance} was invalid, no nodeinfo link"
            );
        }

        HttpResponseMessage nodeInfoResp = await _client.GetAsync(nodeInfoUrl);
        nodeInfoResp.EnsureSuccessStatusCode();

        PartialNodeInfo? nodeInfo = await nodeInfoResp.Content.ReadFromJsonAsync<PartialNodeInfo>();
        return nodeInfo?.Software.Name
            ?? throw new FoxnounsError(
                $"Nodeinfo response for instance {instance} was invalid, no software name"
            );
    }

    private async Task<string> GenerateAuthUrlAsync(
        FediverseApplication app,
        bool forceRefresh,
        string? state = null
    ) =>
        app.InstanceType switch
        {
            FediverseInstanceType.MastodonApi => await GenerateMastodonAuthUrlAsync(
                app,
                forceRefresh,
                state
            ),
            FediverseInstanceType.MisskeyApi => await GenerateMisskeyAuthUrlAsync(
                app,
                forceRefresh
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(app), app.InstanceType, null),
        };

    public async Task<FediverseUser> GetRemoteFediverseUserAsync(
        FediverseApplication app,
        string code,
        string? state = null
    ) =>
        app.InstanceType switch
        {
            FediverseInstanceType.MastodonApi => await GetMastodonUserAsync(app, code, state),
            FediverseInstanceType.MisskeyApi => await GetMisskeyUserAsync(app, code),
            _ => throw new ArgumentOutOfRangeException(nameof(app), app.InstanceType, null),
        };

    private static readonly string[] MastodonSoftwareNames =
    [
        "mastodon",
        "hometown",
        "akkoma",
        "pleroma",
        "iceshrimp.net",
        "iceshrimp",
        "gotosocial",
        "pixelfed",
    ];

    private static readonly string[] MisskeySoftwareNames =
    [
        "misskey",
        "foundkey",
        "calckey",
        "firefish",
        "sharkey",
    ];

    private static bool IsMastodonCompatible(string softwareName) =>
        MastodonSoftwareNames.Any(n =>
            string.Equals(softwareName, n, StringComparison.InvariantCultureIgnoreCase)
        );

    private static bool IsMisskeyCompatible(string softwareName) =>
        MisskeySoftwareNames.Any(n =>
            string.Equals(softwareName, n, StringComparison.InvariantCultureIgnoreCase)
        );

    private record WellKnownResponse([property: J("links")] WellKnownLink[] Links);

    private record WellKnownLink(
        [property: J("rel")] string Rel,
        [property: J("href")] string Href
    );

    private record PartialNodeInfo([property: J("software")] NodeInfoSoftware Software);

    private record NodeInfoSoftware([property: J("name")] string Name);
}
