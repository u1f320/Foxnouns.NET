namespace Foxnouns.Backend.Database.Models;

public class AuthMethod : BaseModel
{
    public required AuthType AuthType { get; init; }
    public required string RemoteId { get; init; }
    public string? RemoteUsername { get; set; }

    public Snowflake UserId { get; init; }
    public User User { get; init; } = null!;

    public Snowflake? FediverseApplicationId { get; init; }
    public FediverseApplication? FediverseApplication { get; init; }
}

public enum AuthType
{
    Discord,
    Google,
    Tumblr,
    Fediverse,
    Email,
}
