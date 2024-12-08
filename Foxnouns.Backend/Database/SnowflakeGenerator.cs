using NodaTime;

namespace Foxnouns.Backend.Database;

public class SnowflakeGenerator : ISnowflakeGenerator
{
    /// <summary>
    /// Singleton instance of SnowflakeGenerator. Where possible, use an injected ISnowflakeGenerator instead
    /// (see IServiceCollection.AddSnowflakeGenerator).
    /// </summary>
    public static SnowflakeGenerator Instance { get; } = new();

    private readonly byte _processId;
    private long _increment;

    public SnowflakeGenerator(int? processId = null)
    {
        processId ??= Environment.ProcessId;
        _processId = (byte)(processId % 32);
        _increment = Random.Shared.NextInt64() % 4096;
    }

    /// <summary>
    /// Generate a snowflake ID for the given timestamp.
    /// </summary>
    /// <param name="time">An optional timestamp. If null, use the current timestamp.</param>
    /// <returns>A new snowflake ID.</returns>
    public Snowflake GenerateSnowflake(Instant? time = null)
    {
        time ??= SystemClock.Instance.GetCurrentInstant();
        long increment = Interlocked.Increment(ref _increment);
        int threadId = Environment.CurrentManagedThreadId % 32;
        long timestamp = time.Value.ToUnixTimeMilliseconds() - Snowflake.Epoch;

        return (timestamp << 22)
            | (uint)(_processId << 17)
            | (uint)(threadId << 12)
            | (increment % 4096);
    }
}

public static class SnowflakeGeneratorServiceExtensions
{
    public static IServiceCollection AddSnowflakeGenerator(
        this IServiceCollection services,
        int? processId = null
    ) => services.AddSingleton<ISnowflakeGenerator>(new SnowflakeGenerator(processId));
}
