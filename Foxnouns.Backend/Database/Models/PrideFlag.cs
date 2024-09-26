namespace Foxnouns.Backend.Database.Models;

public class PrideFlag : BaseModel
{
    public required Snowflake UserId { get; init; }
    public required string Hash { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class UserFlag
{
    public long Id { get; init; }
    public required Snowflake UserId { get; init; }
    public required Snowflake PrideFlagId { get; init; }
    public PrideFlag PrideFlag { get; init; } = null!;
}

public class MemberFlag
{
    public long Id { get; init; }
    public required Snowflake MemberId { get; init; }
    public required Snowflake PrideFlagId { get; init; }
    public PrideFlag PrideFlag { get; init; } = null!;
}