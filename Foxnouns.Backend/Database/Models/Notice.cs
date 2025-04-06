using NodaTime;

namespace Foxnouns.Backend.Database.Models;

public class Notice : BaseModel
{
    public required string Message { get; set; }
    public required Instant StartTime { get; set; }
    public required Instant EndTime { get; set; }

    public Snowflake AuthorId { get; init; }
    public User Author { get; init; } = null!;
}
