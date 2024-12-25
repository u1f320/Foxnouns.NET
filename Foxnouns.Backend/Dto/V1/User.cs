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
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Services.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Foxnouns.Backend.Dto.V1;

public record UserResponse(
    string Id,
    Snowflake IdNew,
    string Sid,
    string Name,
    string? DisplayName,
    string? Bio,
    string? MemberTitle,
    string? Avatar,
    string[] Links,
    FieldEntry[] Names,
    PronounEntry[] Pronouns,
    ProfileField[] Fields,
    PrideFlag[] Flags,
    PartialMember[] Members,
    int? UtcOffset,
    Dictionary<Guid, CustomPreference> CustomPreferences
);

public record CustomPreference(
    string Icon,
    string Tooltip,
    [property: JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        PreferenceSize Size,
    bool Muted,
    bool Favourite
);

public record ProfileField(string Name, FieldEntry[] Entries)
{
    public static ProfileField FromField(
        Field field,
        Dictionary<Snowflake, User.CustomPreference> customPreferences
    ) => new(field.Name, FieldEntry.FromEntries(field.Entries, customPreferences));

    public static ProfileField[] FromFields(
        IEnumerable<Field> fields,
        Dictionary<Snowflake, User.CustomPreference> customPreferences
    ) => fields.Select(f => FromField(f, customPreferences)).ToArray();
}

public record FieldEntry(string Value, string Status)
{
    public static FieldEntry[] FromEntries(
        IEnumerable<Foxnouns.Backend.Database.Models.FieldEntry> entries,
        Dictionary<Snowflake, User.CustomPreference> customPreferences
    ) =>
        entries
            .Select(e => new FieldEntry(
                e.Value,
                V1Utils.TranslateStatus(e.Status, customPreferences)
            ))
            .ToArray();
}

public record PronounEntry(string Pronouns, string? DisplayText, string Status)
{
    public static PronounEntry[] FromPronouns(
        IEnumerable<Pronoun> pronouns,
        Dictionary<Snowflake, User.CustomPreference> customPreferences
    ) =>
        pronouns
            .Select(p => new PronounEntry(
                p.Value,
                p.DisplayText,
                V1Utils.TranslateStatus(p.Status, customPreferences)
            ))
            .ToArray();
}

public record PrideFlag(string Id, Snowflake IdNew, string Hash, string Name, string? Description);
