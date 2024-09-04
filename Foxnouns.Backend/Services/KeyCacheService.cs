using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;

namespace Foxnouns.Backend.Services;

public class KeyCacheService(DatabaseContext db, IClock clock, ILogger logger)
{
    private readonly ILogger _logger = logger.ForContext<KeyCacheService>();

    public Task SetKeyAsync(string key, string value, Duration expireAfter) =>
        SetKeyAsync(key, value, clock.GetCurrentInstant() + expireAfter);

    public async Task SetKeyAsync(string key, string value, Instant expires)
    {
        db.TemporaryKeys.Add(new TemporaryKey
        {
            Expires = expires,
            Key = key,
            Value = value,
        });
        await db.SaveChangesAsync();
    }

    public async Task<string?> GetKeyAsync(string key, bool delete = false)
    {
        var value = await db.TemporaryKeys.FirstOrDefaultAsync(k => k.Key == key);
        if (value == null) return null;

        if (delete)
        {
            await db.TemporaryKeys.Where(k => k.Key == key).ExecuteDeleteAsync();
            await db.SaveChangesAsync();
        }

        return value.Value;
    }

    public async Task DeleteExpiredKeysAsync(CancellationToken ct)
    {
        var count = await db.TemporaryKeys.Where(k => k.Expires < clock.GetCurrentInstant()).ExecuteDeleteAsync(ct);
        if (count != 0) _logger.Information("Removed {Count} expired keys from the database", count);
    }

    public Task SetKeyAsync<T>(string key, T obj, Duration expiresAt) where T : class =>
        SetKeyAsync(key, obj, clock.GetCurrentInstant() + expiresAt);

    public async Task SetKeyAsync<T>(string key, T obj, Instant expires) where T : class
    {
        var value = JsonConvert.SerializeObject(obj);
        await SetKeyAsync(key, value, expires);
    }

    public async Task<T?> GetKeyAsync<T>(string key, bool delete = false) where T : class
    {
        var value = await GetKeyAsync(key, delete: false);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value);
    }
}