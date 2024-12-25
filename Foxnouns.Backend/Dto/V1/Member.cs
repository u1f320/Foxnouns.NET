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

// ReSharper disable NotAccessedPositionalProperty.Global
using Foxnouns.Backend.Database;
using Newtonsoft.Json;

namespace Foxnouns.Backend.Dto.V1;

public record PartialMember(
    string Id,
    Snowflake IdNew,
    string Sid,
    string Name,
    string? DisplayName,
    string? Bio,
    string? Avatar,
    string[] Links,
    FieldEntry[] Names,
    PronounEntry[] Pronouns
);

public record MemberResponse(
    string Id,
    Snowflake IdNew,
    string Sid,
    string Name,
    string? DisplayName,
    string? Bio,
    string? Avatar,
    string[] Links,
    FieldEntry[] Names,
    PronounEntry[] Pronouns,
    ProfileField[] Fields,
    PrideFlag[] Flags,
    PartialUser User,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] bool? Unlisted
);

public record PartialUser(
    string Id,
    Snowflake IdNew,
    string Name,
    string? DisplayName,
    string? Avatar,
    Dictionary<Guid, CustomPreference> CustomPreferences
);
