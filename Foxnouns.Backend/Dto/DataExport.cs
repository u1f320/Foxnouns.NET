// ReSharper disable NotAccessedPositionalProperty.Global
using NodaTime;

namespace Foxnouns.Backend.Dto;

public record DataExportResponse(string? Url, Instant? ExpiresAt);
