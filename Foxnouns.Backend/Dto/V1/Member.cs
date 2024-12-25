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
