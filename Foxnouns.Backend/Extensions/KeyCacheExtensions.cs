using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using NodaTime;

namespace Foxnouns.Backend.Extensions;

public static class KeyCacheExtensions
{
    public static async Task<string> GenerateAuthStateAsync(this KeyCacheService keyCacheSvc)
    {
        var state = OauthUtils.RandomToken();
        await keyCacheSvc.SetKeyAsync($"oauth_state:{state}", "", Duration.FromMinutes(10));
        return state;
    }

    public static async Task ValidateAuthStateAsync(this KeyCacheService keyCacheSvc, string state)
    {
        var val = await keyCacheSvc.GetKeyAsync($"oauth_state:{state}", delete: true);
        if (val == null) throw new ApiError.BadRequest("Invalid OAuth state");
    }
}