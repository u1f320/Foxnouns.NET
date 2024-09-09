using Foxnouns.Backend.Database;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using NodaTime;

namespace Foxnouns.Backend.Extensions;

public static class KeyCacheExtensions
{
    public static async Task<string> GenerateAuthStateAsync(this KeyCacheService keyCacheSvc, CancellationToken ct = default)
    {
        var state = AuthUtils.RandomToken();
        await keyCacheSvc.SetKeyAsync($"oauth_state:{state}", "", Duration.FromMinutes(10), ct);
        return state;
    }

    public static async Task ValidateAuthStateAsync(this KeyCacheService keyCacheSvc, string state, CancellationToken ct = default)
    {
        var val = await keyCacheSvc.GetKeyAsync($"oauth_state:{state}", delete: true, ct);
        if (val == null) throw new ApiError.BadRequest("Invalid OAuth state");
    }

    public static async Task<string> GenerateRegisterEmailStateAsync(this KeyCacheService keyCacheSvc, string email,
        Snowflake? userId = null, CancellationToken ct = default)
    {
        var state = AuthUtils.RandomToken();
        await keyCacheSvc.SetKeyAsync($"email_state:{state}", new RegisterEmailState(email, userId),
            Duration.FromDays(1), ct);
        return state;
    }

    public static async Task<RegisterEmailState?> GetRegisterEmailStateAsync(this KeyCacheService keyCacheSvc,
        string state, CancellationToken ct = default) =>
        await keyCacheSvc.GetKeyAsync<RegisterEmailState>($"email_state:{state}", delete: true, ct);
}

public record RegisterEmailState(string Email, Snowflake? ExistingUserId);