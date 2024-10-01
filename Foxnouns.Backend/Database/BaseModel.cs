namespace Foxnouns.Backend.Database;

public abstract class BaseModel
{
    public required Snowflake Id { get; init; } = SnowflakeGenerator.Instance.GenerateSnowflake();
}
