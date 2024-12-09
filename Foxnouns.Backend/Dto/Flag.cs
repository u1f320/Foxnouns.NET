// ReSharper disable NotAccessedPositionalProperty.Global
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Utils;

namespace Foxnouns.Backend.Dto;

public record PrideFlagResponse(Snowflake Id, string? ImageUrl, string Name, string? Description);

public record CreateFlagRequest(string Name, string Image, string? Description);

public record CreateFlagResponse(Snowflake Id, string Name, string? Description);

public class UpdateFlagRequest : PatchRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
}
