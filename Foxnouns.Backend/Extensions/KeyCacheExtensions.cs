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
        string state = AuthUtils.RandomToken().Replace('+', '-').Replace('/', '_');
        await keyCacheService.SetKeyAsync($"oauth_state:{state}", "", Duration.FromMinutes(10), ct);
        return state;
    }

    public static async Task ValidateAuthStateAsync(
        this KeyCacheService keyCacheService,
        string state,
        CancellationToken ct = default
    )
    {
        string? val = await keyCacheService.GetKeyAsync($"oauth_state:{state}", ct: ct);
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
        string state = AuthUtils.RandomToken().Replace('+', '-').Replace('/', '_');
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
        string state = AuthUtils.RandomToken();
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
    ) => await keyCacheService.GetKeyAsync<AddExtraAccountState>($"add_account:{state}", true, ct);

    public static async Task<string> GenerateForgotPasswordStateAsync(
        this KeyCacheService keyCacheService,
        string email,
        Snowflake userId,
        CancellationToken ct = default
    )
    {
        string state = AuthUtils.RandomToken();
        await keyCacheService.SetKeyAsync(
            $"forgot_password:{state}",
            new ForgotPasswordState(email, userId),
            Duration.FromHours(1),
            ct
        );
        return state;
    }

    public static async Task<ForgotPasswordState?> GetForgotPasswordStateAsync(
        this KeyCacheService keyCacheService,
        string state,
        CancellationToken ct = default
    ) =>
        await keyCacheService.GetKeyAsync<ForgotPasswordState>(
            $"forgot_password:{state}",
            true,
            ct
        );
}

public record RegisterEmailState(
    string Email,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] Snowflake? ExistingUserId
);

public record ForgotPasswordState(string Email, Snowflake UserId);

public record AddExtraAccountState(AuthType AuthType, Snowflake UserId, string? Instance = null);
