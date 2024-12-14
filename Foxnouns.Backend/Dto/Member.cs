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
// ReSharper disable UnusedAutoPropertyAccessor.Global
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Newtonsoft.Json;

namespace Foxnouns.Backend.Dto;

public record CreateMemberRequest(
    string Name,
    string? DisplayName,
    string? Bio,
    string? Avatar,
    bool? Unlisted,
    string[]? Links,
    List<FieldEntry>? Names,
    List<Pronoun>? Pronouns,
    List<Field>? Fields
);

public class UpdateMemberRequest : PatchRequest
{
    public string? Name { get; init; }
    public string? DisplayName { get; init; }
    public string? Bio { get; init; }
    public string? Avatar { get; init; }
    public string[]? Links { get; init; }
    public FieldEntry[]? Names { get; init; }
    public Pronoun[]? Pronouns { get; init; }
    public Field[]? Fields { get; init; }
    public Snowflake[]? Flags { get; init; }
    public bool? Unlisted { get; init; }
}

public record PartialMember(
    Snowflake Id,
    string Sid,
    string Name,
    string DisplayName,
    string? Bio,
    string? AvatarUrl,
    IEnumerable<FieldEntry> Names,
    IEnumerable<Pronoun> Pronouns,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] bool? Unlisted
);

public record MemberResponse(
    Snowflake Id,
    string Sid,
    string Name,
    string DisplayName,
    string? Bio,
    string? AvatarUrl,
    string[] Links,
    IEnumerable<FieldEntry> Names,
    IEnumerable<Pronoun> Pronouns,
    IEnumerable<Field> Fields,
    IEnumerable<PrideFlagResponse> Flags,
    PartialUser User,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] bool? Unlisted
);
