using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;

namespace Foxnouns.DataMigrator.Models;

public class GoUser
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public string? DisplayName { get; init; }
    public string? Bio { get; init; }
    public string[]? Links { get; init; }
    public string? Discord { get; init; }
    public string? DiscordUsername { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public bool? SelfDelete { get; init; }
    public string? DeleteReason { get; init; }
    public GoFieldEntry[] Names { get; init; } = [];
    public GoPronounEntry[] Pronouns { get; init; } = [];
    public string? Avatar { get; init; }
    public string? Fediverse { get; init; }
    public string? FediverseUsername { get; init; }
    public int? FediverseAppId { get; init; }
    public bool IsAdmin { get; init; }
    public string? MemberTitle { get; init; }
    public bool ListPrivate { get; init; }
    public string? Tumblr { get; init; }
    public string? TumblrUsername { get; init; }
    public string? Google { get; init; }
    public string? GoogleUsername { get; init; }
    public Dictionary<string, GoCustomPreference> CustomPreferences { get; init; } = [];
    public DateTimeOffset LastActive { get; init; }
    public required string Sid { get; init; }
    public DateTimeOffset LastSidReroll { get; init; }
    public string? Timezone { get; init; }
    public Snowflake SnowflakeId { get; init; }
}

public class GoUserField
{
    public required string UserId { get; init; }
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required GoFieldEntry[] Entries { get; init; }
}

public class GoPrideFlag
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string Hash { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required Snowflake SnowflakeId { get; init; }
}

public class GoProfileFlag
{
    public string? UserId { get; init; }
    public string? MemberId { get; init; }
    public required long Id { get; init; }
    public required string FlagId { get; init; }
}

public class GoFediverseApp
{
    public required int Id { get; init; }
    public required string Instance { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string InstanceType { get; init; }

    public FediverseInstanceType TypeToEnum() =>
        InstanceType switch
        {
            "sharkey" => FediverseInstanceType.MisskeyApi,
            "firefish" => FediverseInstanceType.MisskeyApi,
            "pixelfed" => FediverseInstanceType.MastodonApi,
            "mastodon" => FediverseInstanceType.MastodonApi,
            "pleroma" => FediverseInstanceType.MastodonApi,
            "akkoma" => FediverseInstanceType.MastodonApi,
            "misskey" => FediverseInstanceType.MastodonApi,
            "gotosocial" => FediverseInstanceType.MastodonApi,
            "calckey" => FediverseInstanceType.MisskeyApi,
            "foundkey" => FediverseInstanceType.MisskeyApi,
            // this should never happen but if it does we just fall back to the mastodon api
            // basically everything but misskey forks implement it anyway :3
            _ => FediverseInstanceType.MastodonApi,
        };
}

public record GoFieldEntry(string Value, string Status);

public record GoPronounEntry(string Pronouns, string? DisplayText, string Status);

public record GoCustomPreference(
    string Icon,
    string Tooltip,
    string PreferenceSize,
    bool Muted,
    bool Favourite
);
