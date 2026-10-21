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
    extension(KeyCacheService keyCacheService)
    {
        public async Task<string> GenerateAuthStateAsync()
        {
            string state = AuthUtils.RandomToken();
            await keyCacheService.SetKeyAsync($"oauth_state:{state}", "", Duration.FromMinutes(10));
            return state;
        }

        public async Task ValidateAuthStateAsync(string state)
        {
            string? val = await keyCacheService.GetKeyAsync($"oauth_state:{state}");
            if (val == null)
                throw new ApiError.BadRequest("Invalid OAuth state");
        }

        public async Task<string> GenerateRegisterEmailStateAsync(
            string email,
            Snowflake? userId = null
        )
        {
            string state = AuthUtils.RandomToken();
            await keyCacheService.SetKeyAsync(
                $"email_state:{state}",
                new RegisterEmailState(email, userId),
                Duration.FromDays(1)
            );
            return state;
        }

        public async Task<RegisterEmailState?> GetRegisterEmailStateAsync(string state) =>
            await keyCacheService.GetKeyAsync<RegisterEmailState>($"email_state:{state}");

        public async Task<string> GenerateAddExtraAccountStateAsync(
            AuthType authType,
            Snowflake userId,
            string? instance = null
        )
        {
            string state = AuthUtils.RandomToken();
            await keyCacheService.SetKeyAsync(
                $"add_account:{state}",
                new AddExtraAccountState(authType, userId, instance),
                Duration.FromDays(1)
            );
            return state;
        }

        public async Task<AddExtraAccountState?> GetAddExtraAccountStateAsync(string state) =>
            await keyCacheService.GetKeyAsync<AddExtraAccountState>($"add_account:{state}", true);

        public async Task<string> GenerateForgotPasswordStateAsync(string email, Snowflake userId)
        {
            string state = AuthUtils.RandomToken();
            await keyCacheService.SetKeyAsync(
                $"forgot_password:{state}",
                new ForgotPasswordState(email, userId),
                Duration.FromHours(1)
            );
            return state;
        }

        public async Task<ForgotPasswordState?> GetForgotPasswordStateAsync(
            string state,
            bool delete = true
        ) =>
            await keyCacheService.GetKeyAsync<ForgotPasswordState>(
                $"forgot_password:{state}",
                delete
            );
    }
}

public record RegisterEmailState(
    string Email,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] Snowflake? ExistingUserId
);

public record ForgotPasswordState(string Email, Snowflake UserId);

public record AddExtraAccountState(AuthType AuthType, Snowflake UserId, string? Instance = null);
