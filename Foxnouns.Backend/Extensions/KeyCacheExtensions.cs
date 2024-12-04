using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Newtonsoft.Json;
using NodaTime;

namespace Foxnouns.Backend.Extensions;

public static class KeyCacheExtensions
{
    public static async Task<string> GenerateAuthStateAsync(
        this KeyCacheService keyCacheService,
        CancellationToken ct = default
    )
    {
        var state = AuthUtils.RandomToken().Replace('+', '-').Replace('/', '_');
        await keyCacheService.SetKeyAsync($"oauth_state:{state}", "", Duration.FromMinutes(10), ct);
        return state;
    }

    public static async Task ValidateAuthStateAsync(
        this KeyCacheService keyCacheService,
        string state,
        CancellationToken ct = default
    )
    {
        var val = await keyCacheService.GetKeyAsync($"oauth_state:{state}", ct: ct);
        if (val == null)
            throw new ApiError.BadRequest("Invalid OAuth state");
    }

    public static async Task<string> GenerateRegisterEmailStateAsync(
        this KeyCacheService keyCacheService,
        string email,
        Snowflake? userId = null,
        CancellationToken ct = default
    )
    {
        // This state is used in links, not just as JSON values, so make it URL-safe
        var state = AuthUtils.RandomToken().Replace('+', '-').Replace('/', '_');
        await keyCacheService.SetKeyAsync(
            $"email_state:{state}",
            new RegisterEmailState(email, userId),
            Duration.FromDays(1),
            ct
        );
        return state;
    }

    public static async Task<RegisterEmailState?> GetRegisterEmailStateAsync(
        this KeyCacheService keyCacheService,
        string state,
        CancellationToken ct = default
    ) => await keyCacheService.GetKeyAsync<RegisterEmailState>($"email_state:{state}", ct: ct);

    public static async Task<string> GenerateAddExtraAccountStateAsync(
        this KeyCacheService keyCacheService,
        AuthType authType,
        Snowflake userId,
        string? instance = null,
        CancellationToken ct = default
    )
    {
        var state = AuthUtils.RandomToken();
        await keyCacheService.SetKeyAsync(
            $"add_account:{state}",
            new AddExtraAccountState(authType, userId, instance),
            Duration.FromDays(1),
            ct
        );
        return state;
    }

    public static async Task<AddExtraAccountState?> GetAddExtraAccountStateAsync(
        this KeyCacheService keyCacheService,
        string state,
        CancellationToken ct = default
    ) =>
        await keyCacheService.GetKeyAsync<AddExtraAccountState>(
            $"add_account:{state}",
            delete: true,
            ct
        );
}

public record RegisterEmailState(
    string Email,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] Snowflake? ExistingUserId
);

public record AddExtraAccountState(AuthType AuthType, Snowflake UserId, string? Instance = null);
