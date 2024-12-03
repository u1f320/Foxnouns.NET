namespace Foxnouns.Backend.Database.Models;

public class FediverseApplication : BaseModel
{
    public required string Domain { get; init; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required FediverseInstanceType InstanceType { get; set; }
}

public enum FediverseInstanceType
{
    MastodonApi,
    MisskeyApi,
}
