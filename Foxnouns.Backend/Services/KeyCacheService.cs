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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;
using StackExchange.Redis;

namespace Foxnouns.Backend.Services;

public class KeyCacheService(Config config)
{
    public ConnectionMultiplexer Multiplexer { get; } =
        ConnectionMultiplexer.Connect(config.Database.Redis);

    public async Task SetKeyAsync(string key, string value, Duration expireAfter) =>
        await Multiplexer
            .GetDatabase()
            .StringSetAsync(key, value, expiry: expireAfter.ToTimeSpan());

    public async Task<string?> GetKeyAsync(string key, bool delete = false) =>
        delete
            ? await Multiplexer.GetDatabase().StringGetDeleteAsync(key)
            : await Multiplexer.GetDatabase().StringGetAsync(key);

    public async Task DeleteKeyAsync(string key) =>
        await Multiplexer.GetDatabase().KeyDeleteAsync(key);

    public async Task SetKeyAsync<T>(string key, T obj, Duration expiresAt)
        where T : class
    {
        string value = JsonConvert.SerializeObject(obj);
        await SetKeyAsync(key, value, expiresAt);
    }

    public async Task<T?> GetKeyAsync<T>(string key, bool delete = false)
        where T : class
    {
        string? value = await GetKeyAsync(key, delete);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value);
    }
}
