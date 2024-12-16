using Foxnouns.Backend.Database;

namespace Foxnouns.DataMigrator.Models;

public class GoMember
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Bio { get; init; }
    public string[]? Links { get; init; }
    public string? DisplayName { get; init; }
    public GoFieldEntry[] Names { get; init; } = [];
    public GoPronounEntry[] Pronouns { get; init; } = [];
    public string? Avatar { get; init; }
    public required bool Unlisted { get; init; }
    public required string Sid { get; init; }
    public required Snowflake SnowflakeId { get; init; }
}

public class GoMemberField
{
    public required string MemberId { get; init; }
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required GoFieldEntry[] Entries { get; init; }
}
