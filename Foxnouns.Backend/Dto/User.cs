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
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Utils;
using Newtonsoft.Json;
using NodaTime;

namespace Foxnouns.Backend.Dto;

public record UserResponse(
    Snowflake Id,
    string Sid,
    string Username,
    string? DisplayName,
    string? Bio,
    string? MemberTitle,
    string? AvatarUrl,
    string[] Links,
    IEnumerable<FieldEntry> Names,
    IEnumerable<Pronoun> Pronouns,
    IEnumerable<Field> Fields,
    Dictionary<Snowflake, User.CustomPreference> CustomPreferences,
    IEnumerable<PrideFlagResponse> Flags,
    int? UtcOffset,
    [property: JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))] UserRole Role,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        IEnumerable<PartialMember>? Members,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        IEnumerable<AuthMethodResponse>? AuthMethods,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] bool? MemberListHidden,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] Instant? LastActive,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] Instant? LastSidReroll,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? Timezone,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] bool? Deleted
);

public record AuthMethodResponse(
    Snowflake Id,
    [property: JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))] AuthType Type,
    string RemoteId,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string? RemoteUsername
);

public record PartialUser(
    Snowflake Id,
    string Sid,
    string Username,
    string? DisplayName,
    string? AvatarUrl,
    Dictionary<Snowflake, User.CustomPreference> CustomPreferences
);

public class UpdateUserSettingsRequest : PatchRequest
{
    public bool? DarkMode { get; init; }
}

public class CustomPreferenceUpdateRequest
{
    public Snowflake? Id { get; init; }
    public required string Icon { get; set; }
    public required string Tooltip { get; set; }
    public PreferenceSize Size { get; set; }
    public bool Muted { get; set; }
    public bool Favourite { get; set; }
}

public class UpdateUserRequest : PatchRequest
{
    public string? Username { get; init; }
    public string? DisplayName { get; init; }
    public string? Bio { get; init; }
    public string? Avatar { get; init; }
    public string[]? Links { get; init; }
    public FieldEntry[]? Names { get; init; }
    public Pronoun[]? Pronouns { get; init; }
    public Field[]? Fields { get; init; }
    public Snowflake[]? Flags { get; init; }
    public string? MemberTitle { get; init; }
    public bool? MemberListHidden { get; init; }
    public string? Timezone { get; init; }
}
