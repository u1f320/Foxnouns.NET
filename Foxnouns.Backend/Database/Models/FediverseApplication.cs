using NodaTime;

namespace Foxnouns.Backend.Database.Models;

public class FediverseApplication : BaseModel
{
    public required string Domain { get; init; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required FediverseInstanceType InstanceType { get; set; }

    // These are for ensuring the application is still valid.
    public string? AccessToken { get; set; }
    public Instant? TokenValidUntil { get; set; }
}

public enum FediverseInstanceType
{
    MastodonApi,
    MisskeyApi,
}
