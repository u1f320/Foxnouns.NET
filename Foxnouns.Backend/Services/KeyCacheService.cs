using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;

namespace Foxnouns.Backend.Services;

public class KeyCacheService(DatabaseContext db, IClock clock, ILogger logger)
{
    private readonly ILogger _logger = logger.ForContext<KeyCacheService>();

    public Task SetKeyAsync(
        string key,
        string value,
        Duration expireAfter,
        CancellationToken ct = default
    ) => SetKeyAsync(key, value, clock.GetCurrentInstant() + expireAfter, ct);

    public async Task SetKeyAsync(
        string key,
        string value,
        Instant expires,
        CancellationToken ct = default
    )
    {
        db.TemporaryKeys.Add(
            new TemporaryKey
            {
                Expires = expires,
                Key = key,
                Value = value,
            }
        );
        await db.SaveChangesAsync(ct);
    }

    public async Task<string?> GetKeyAsync(
        string key,
        bool delete = false,
        CancellationToken ct = default
    )
    {
        TemporaryKey? value = await db.TemporaryKeys.FirstOrDefaultAsync(k => k.Key == key, ct);
        if (value == null)
            return null;

        if (delete)
            await db.TemporaryKeys.Where(k => k.Key == key).ExecuteDeleteAsync(ct);

        return value.Value;
    }

    public async Task DeleteKeyAsync(string key, CancellationToken ct = default) =>
        await db.TemporaryKeys.Where(k => k.Key == key).ExecuteDeleteAsync(ct);

    public async Task DeleteExpiredKeysAsync(CancellationToken ct)
    {
        int count = await db
            .TemporaryKeys.Where(k => k.Expires < clock.GetCurrentInstant())
            .ExecuteDeleteAsync(ct);
        if (count != 0)
            _logger.Information("Removed {Count} expired keys from the database", count);
    }

    public Task SetKeyAsync<T>(
        string key,
        T obj,
        Duration expiresAt,
        CancellationToken ct = default
    )
        where T : class => SetKeyAsync(key, obj, clock.GetCurrentInstant() + expiresAt, ct);

    public async Task SetKeyAsync<T>(
        string key,
        T obj,
        Instant expires,
        CancellationToken ct = default
    )
        where T : class
    {
        string value = JsonConvert.SerializeObject(obj);
        await SetKeyAsync(key, value, expires, ct);
    }

    public async Task<T?> GetKeyAsync<T>(
        string key,
        bool delete = false,
        CancellationToken ct = default
    )
        where T : class
    {
        string? value = await GetKeyAsync(key, delete, ct);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value);
    }
}
