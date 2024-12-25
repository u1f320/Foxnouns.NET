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
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto.V1;
using Microsoft.EntityFrameworkCore;
using FieldEntry = Foxnouns.Backend.Dto.V1.FieldEntry;
using PrideFlag = Foxnouns.Backend.Dto.V1.PrideFlag;

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

    public async Task<UserResponse> RenderUserAsync(
        User user,
        Token? token = null,
        bool renderMembers = true,
        bool renderFlags = true,
        CancellationToken ct = default
    )
    {
        bool isSelfUser = user.Id == token?.UserId;
        renderMembers = renderMembers && (isSelfUser || !user.ListHidden);

        // Only fetch members if we're rendering members (duh)
        List<Member> members = renderMembers
            ? await db.Members.Where(m => m.UserId == user.Id).OrderBy(m => m.Name).ToListAsync(ct)
            : [];

        List<UserFlag> flags = renderFlags
            ? await db.UserFlags.Where(f => f.UserId == user.Id).OrderBy(f => f.Id).ToListAsync(ct)
            : [];

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
            Names: FieldEntry.FromEntries(user.Names, user.CustomPreferences),
            Pronouns: PronounEntry.FromPronouns(user.Pronouns, user.CustomPreferences),
            Fields: ProfileField.FromFields(user.Fields, user.CustomPreferences),
            Flags: flags
                .Where(f => f.PrideFlag.Hash != null)
                .Select(f => new PrideFlag(
                    f.PrideFlag.LegacyId,
                    f.PrideFlag.Id,
                    f.PrideFlag.Hash!,
                    f.PrideFlag.Name,
                    f.PrideFlag.Description
                ))
                .ToArray(),
            Members: members.Select(m => RenderPartialMember(m, user.CustomPreferences)).ToArray(),
            utcOffset,
            CustomPreferences: RenderCustomPreferences(user.CustomPreferences)
        );
    }

    private static Dictionary<Guid, CustomPreference> RenderCustomPreferences(
        Dictionary<Snowflake, User.CustomPreference> customPreferences
    ) =>
        customPreferences
            .Select(x =>
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
            .ToDictionary();

    private static PartialMember RenderPartialMember(
        Member m,
        Dictionary<Snowflake, User.CustomPreference> customPreferences
    ) =>
        new(
            m.LegacyId,
            m.Id,
            m.Sid,
            m.Name,
            m.DisplayName,
            m.Bio,
            m.Avatar,
            m.Links,
            Names: FieldEntry.FromEntries(m.Names, customPreferences),
            Pronouns: PronounEntry.FromPronouns(m.Pronouns, customPreferences)
        );

    public static PartialUser RenderPartialUser(User user) =>
        new(
            user.LegacyId,
            user.Id,
            user.Username,
            user.DisplayName,
            user.Avatar,
            CustomPreferences: RenderCustomPreferences(user.CustomPreferences)
        );
}
