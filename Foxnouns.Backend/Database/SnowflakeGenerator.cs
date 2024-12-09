// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
