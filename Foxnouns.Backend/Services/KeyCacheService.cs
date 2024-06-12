using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Services;

public class KeyCacheService(DatabaseContext db, IClock clock, ILogger logger)
{
    public Task SetKeyAsync(string key, string value, Duration expireAfter) =>
        db.SetKeyAsync(key, value, clock.GetCurrentInstant() + expireAfter);

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

    public async Task DeleteExpiredKeysAsync()
    {
        var count = await db.TemporaryKeys.Where(k => k.Expires < clock.GetCurrentInstant()).ExecuteDeleteAsync();
        if (count != 0) logger.Information("Removed {Count} expired keys from the database", count);
    }

    public async Task<string> GenerateAuthStateAsync()
    {
        var state = OauthUtils.RandomToken();
        await SetKeyAsync($"oauth_state:{state}", "", Duration.FromMinutes(10));
        return state;
    }

    public async Task ValidateAuthStateAsync(string state)
    {
        var val = await GetKeyAsync($"oauth_state:{state}", delete: true);
        if (val == null) throw new ApiError.BadRequest("Invalid OAuth state");
    }
}