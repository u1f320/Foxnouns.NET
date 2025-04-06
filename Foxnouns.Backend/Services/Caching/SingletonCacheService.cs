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
using Foxnouns.Backend.Database;
using NodaTime;

namespace Foxnouns.Backend.Services.Caching;

public abstract class SingletonCacheService<T>(
    IServiceProvider serviceProvider,
    IClock clock,
    ILogger logger
)
    where T : class
{
    private T? _item;
    private Instant _lastUpdated = Instant.MinValue;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger _logger = logger.ForContext<SingletonCacheService<T>>();

    public virtual Duration MaxAge { get; init; } = Duration.FromMinutes(5);

    public virtual Func<DatabaseContext, CancellationToken, Task<T?>> FetchFunc { get; init; } =
        (_, __) => Task.FromResult<T?>(null);

    public async Task<T?> GetAsync(CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            if (_lastUpdated > clock.GetCurrentInstant() - MaxAge)
            {
                return _item;
            }

            _logger.Debug("Cached item of type {Type} is expired, fetching it.", typeof(T));

            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            await using DatabaseContext db =
                scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            T? item = await FetchFunc(db, ct);
            _item = item;
            _lastUpdated = clock.GetCurrentInstant();
            return item;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
