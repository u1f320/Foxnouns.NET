using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Foxnouns.Backend.Database.Models;

public class DataExport : BaseModel
{
    public Snowflake UserId { get; init; }
    public User User { get; init; } = null!;
    public required string Filename { get; init; }

    [NotMapped]
    public static readonly Duration Expiration = Duration.FromDays(15);
}
