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
