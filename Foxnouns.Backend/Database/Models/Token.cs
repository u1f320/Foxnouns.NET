using NodaTime;

namespace Foxnouns.Backend.Database.Models;

public class Token : BaseModel
{
    public required Instant ExpiresAt { get; init; }
    public required string[] Scopes { get; init; }
    public bool ManuallyExpired { get; set; }

    public bool IsExpired => ManuallyExpired || ExpiresAt < SystemClock.Instance.GetCurrentInstant();

    public Snowflake UserId { get; init; }
    public User User { get; init; } = null!;
}