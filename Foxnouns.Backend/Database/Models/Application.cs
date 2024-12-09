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
using System.Security.Cryptography;
using Foxnouns.Backend.Utils;

namespace Foxnouns.Backend.Database.Models;

public class Application : BaseModel
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string Name { get; init; }
    public required string[] Scopes { get; init; }
    public required string[] RedirectUris { get; init; }

    public static Application Create(
        ISnowflakeGenerator snowflakeGenerator,
        string name,
        string[] scopes,
        string[] redirectUrls
    )
    {
        string clientId = RandomNumberGenerator.GetHexString(32, true);
        string clientSecret = AuthUtils.RandomToken();

        if (scopes.Except(AuthUtils.ApplicationScopes).Any())
        {
            throw new ArgumentException(
                "Invalid scopes passed to Application.Create",
                nameof(scopes)
            );
        }

        if (redirectUrls.Any(s => !AuthUtils.ValidateRedirectUri(s)))
        {
            throw new ArgumentException(
                "Invalid redirect URLs passed to Application.Create",
                nameof(redirectUrls)
            );
        }

        return new Application
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            ClientId = clientId,
            ClientSecret = clientSecret,
            Name = name,
            Scopes = scopes,
            RedirectUris = redirectUrls,
        };
    }
}
