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
using System.Globalization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Foxnouns.Backend.Utils;

/// <summary>
/// A custom StringEnumConverter that converts enum members to SCREAMING_SNAKE_CASE, rather than CamelCase as is the default.
/// Newtonsoft.Json doesn't provide a screaming snake case naming strategy, so we just wrap the normal snake case one and convert it to uppercase.
/// </summary>
public class ScreamingSnakeCaseEnumConverter()
    : StringEnumConverter(new ScreamingSnakeCaseNamingStrategy(), false)
{
    private class ScreamingSnakeCaseNamingStrategy : SnakeCaseNamingStrategy
    {
        protected override string ResolvePropertyName(string name) =>
            base.ResolvePropertyName(name).ToUpper(CultureInfo.InvariantCulture);
    }
}
