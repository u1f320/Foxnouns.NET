using NodaTime;

namespace Foxnouns.Backend.Database.Models;

public class Cache
{
    public long Id { get; init; }
    public required string Key { get; init; }
    public required string Value { get; set; }
    public Instant Expires { get; init; }
}