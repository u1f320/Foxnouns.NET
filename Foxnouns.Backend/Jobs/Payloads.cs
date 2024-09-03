using Foxnouns.Backend.Database;

namespace Foxnouns.Backend.Jobs;

public record AvatarUpdatePayload(Snowflake Id, string? NewAvatar);