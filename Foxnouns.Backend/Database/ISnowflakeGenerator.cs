using NodaTime;

namespace Foxnouns.Backend.Database;

public interface ISnowflakeGenerator
{
    Snowflake GenerateSnowflake(Instant? time = null);
}