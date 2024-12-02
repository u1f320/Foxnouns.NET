using Foxnouns.Backend.Database;

namespace Foxnouns.Backend.Jobs;

public record AvatarUpdatePayload(Snowflake Id, string? NewAvatar);

public record CreateFlagPayload(
    Snowflake Id,
    Snowflake UserId,
    string Name,
    string ImageData,
    string? Description
);

public record CreateDataExportPayload(Snowflake UserId);
