using NodaTime;

namespace Foxnouns.Backend.Database.Models;

public class Token : BaseModel
{
    public required byte[] Hash { get; init; }
    public required Instant ExpiresAt { get; init; }
    public required string[] Scopes { get; init; }
    public bool ManuallyExpired { get; set; }

    public Snowflake UserId { get; init; }
    public User User { get; init; } = null!;

    public Snowflake ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
}