// ReSharper disable NotAccessedPositionalProperty.Global
using Foxnouns.Backend.Database;

namespace Foxnouns.Backend.Dto;

public record RequestDataRequest(string? Token, string Method, string Path);

public record RequestDataResponse(Snowflake? UserId, string Template);
