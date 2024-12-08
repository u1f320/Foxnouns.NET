// ReSharper disable NotAccessedPositionalProperty.Global
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
