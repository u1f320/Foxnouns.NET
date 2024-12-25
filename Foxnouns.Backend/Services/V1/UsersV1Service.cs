using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto.V1;
using Microsoft.EntityFrameworkCore;
using FieldEntry = Foxnouns.Backend.Dto.V1.FieldEntry;

namespace Foxnouns.Backend.Services.V1;

public class UsersV1Service(DatabaseContext db)
{
    public async Task<User> ResolveUserAsync(
        string userRef,
        Token? token,
        CancellationToken ct = default
    )
    {
        if (userRef == "@me")
        {
            if (token == null)
            {
                throw new ApiError.Unauthorized(
                    "This endpoint requires an authenticated user.",
                    ErrorCode.AuthenticationRequired
                );
            }

            return await db.Users.FirstAsync(u => u.Id == token.UserId, ct);
        }

        User? user;
        if (Snowflake.TryParse(userRef, out Snowflake? sf))
        {
            user = await db.Users.FirstOrDefaultAsync(u => u.Id == sf && !u.Deleted, ct);
            if (user != null)
                return user;
        }

        user = await db.Users.FirstOrDefaultAsync(u => u.LegacyId == userRef && !u.Deleted, ct);
        if (user != null)
            return user;

        user = await db.Users.FirstOrDefaultAsync(u => u.Username == userRef && !u.Deleted, ct);
        if (user != null)
            return user;

        throw new ApiError.NotFound(
            "No user with that ID or username found.",
            ErrorCode.UserNotFound
        );
    }

    public async Task<UserResponse> RenderUserAsync(User user)
    {
        int? utcOffset = null;
        if (
            user.Timezone != null
            && TimeZoneInfo.TryFindSystemTimeZoneById(user.Timezone, out TimeZoneInfo? tz)
        )
        {
            utcOffset = (int)tz.GetUtcOffset(DateTimeOffset.UtcNow).TotalSeconds;
        }

        return new UserResponse(
            user.LegacyId,
            user.Id,
            user.Sid,
            user.Username,
            user.DisplayName,
            user.Bio,
            user.MemberTitle,
            user.Avatar,
            user.Links,
            FieldEntry.FromEntries(user.Names, user.CustomPreferences),
            PronounEntry.FromPronouns(user.Pronouns, user.CustomPreferences),
            ProfileField.FromFields(user.Fields, user.CustomPreferences),
            utcOffset,
            user.CustomPreferences.Select(x =>
                    (
                        x.Value.LegacyId,
                        new CustomPreference(
                            x.Value.Icon,
                            x.Value.Tooltip,
                            x.Value.Size,
                            x.Value.Muted,
                            x.Value.Favourite
                        )
                    )
                )
                .ToDictionary()
        );
    }
}
