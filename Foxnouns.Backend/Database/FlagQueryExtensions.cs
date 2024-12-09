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
using Foxnouns.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Database;

public static class FlagQueryExtensions
{
    private static async Task<List<PrideFlag>> GetFlagsAsync(
        this DatabaseContext db,
        Snowflake userId
    ) => await db.PrideFlags.Where(f => f.UserId == userId).OrderBy(f => f.Id).ToListAsync();

    /// <summary>
    /// Sets the user's profile flags to the given IDs. Returns a validation error if any of the flag IDs are unknown
    /// or if too many IDs are given. Duplicates are allowed.
    /// </summary>
    public static async Task<ValidationError?> SetUserFlagsAsync(
        this DatabaseContext db,
        Snowflake userId,
        Snowflake[] flagIds
    )
    {
        List<UserFlag> currentFlags = await db
            .UserFlags.Where(f => f.UserId == userId)
            .ToListAsync();
        foreach (UserFlag flag in currentFlags)
            db.UserFlags.Remove(flag);

        // If there's no new flags to set, we're done
        if (flagIds.Length == 0)
            return null;
        if (flagIds.Length > 100)
            return ValidationError.LengthError("Too many profile flags", 0, 100, flagIds.Length);

        List<PrideFlag> flags = await db.GetFlagsAsync(userId);
        Snowflake[] unknownFlagIds = flagIds.Where(id => flags.All(f => f.Id != id)).ToArray();
        if (unknownFlagIds.Length != 0)
            return ValidationError.GenericValidationError("Unknown flag IDs", unknownFlagIds);

        IEnumerable<UserFlag> userFlags = flagIds.Select(id => new UserFlag
        {
            PrideFlagId = id,
            UserId = userId,
        });
        db.UserFlags.AddRange(userFlags);

        return null;
    }

    public static async Task<ValidationError?> SetMemberFlagsAsync(
        this DatabaseContext db,
        Snowflake userId,
        Snowflake memberId,
        Snowflake[] flagIds
    )
    {
        List<MemberFlag> currentFlags = await db
            .MemberFlags.Where(f => f.MemberId == memberId)
            .ToListAsync();
        foreach (MemberFlag flag in currentFlags)
            db.MemberFlags.Remove(flag);

        if (flagIds.Length == 0)
            return null;
        if (flagIds.Length > 100)
            return ValidationError.LengthError("Too many profile flags", 0, 100, flagIds.Length);

        List<PrideFlag> flags = await db.GetFlagsAsync(userId);
        Snowflake[] unknownFlagIds = flagIds.Where(id => flags.All(f => f.Id != id)).ToArray();
        if (unknownFlagIds.Length != 0)
            return ValidationError.GenericValidationError("Unknown flag IDs", unknownFlagIds);

        IEnumerable<MemberFlag> memberFlags = flagIds.Select(id => new MemberFlag
        {
            PrideFlagId = id,
            MemberId = memberId,
        });
        db.MemberFlags.AddRange(memberFlags);

        return null;
    }
}
