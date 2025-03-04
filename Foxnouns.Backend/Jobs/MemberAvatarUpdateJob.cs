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
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Hangfire;

namespace Foxnouns.Backend.Jobs;

public class MemberAvatarUpdateJob(
    DatabaseContext db,
    ObjectStorageService objectStorageService,
    ILogger logger
)
{
    private readonly ILogger _logger = logger.ForContext<MemberAvatarUpdateJob>();

    public static void Enqueue(AvatarUpdatePayload payload)
    {
        BackgroundJob.Enqueue<MemberAvatarUpdateJob>(j => j.InvokeAsync(payload));
    }

    public async Task InvokeAsync(AvatarUpdatePayload payload)
    {
        if (payload.NewAvatar != null)
            await UpdateMemberAvatarAsync(payload.Id, payload.NewAvatar);
        else
            await ClearMemberAvatarAsync(payload.Id);
    }

    private async Task UpdateMemberAvatarAsync(Snowflake id, string newAvatar)
    {
        _logger.Debug("Updating avatar for member {MemberId}", id);

        Member? member = await db.Members.FindAsync(id);
        if (member == null)
        {
            _logger.Warning(
                "Update avatar job queued for {MemberId} but no member with that ID exists",
                id
            );
            return;
        }

        try
        {
            (string? hash, Stream? image) = await ImageObjectExtensions.ConvertBase64UriToImage(
                newAvatar,
                512,
                true
            );
            string? prevHash = member.Avatar;

            await objectStorageService.PutObjectAsync(Path(id, hash), image, "image/webp");

            member.Avatar = hash;
            await db.SaveChangesAsync();

            if (prevHash != null && prevHash != hash)
                await objectStorageService.RemoveObjectAsync(Path(id, prevHash));

            _logger.Information("Updated avatar for member {MemberId}", id);
        }
        catch (ArgumentException ae)
        {
            _logger.Warning(
                "Invalid data URI for new avatar for member {MemberId}: {Reason}",
                id,
                ae.Message
            );
        }
    }

    private async Task ClearMemberAvatarAsync(Snowflake id)
    {
        _logger.Debug("Clearing avatar for member {MemberId}", id);

        Member? member = await db.Members.FindAsync(id);
        if (member == null)
        {
            _logger.Warning(
                "Clear avatar job queued for {MemberId} but no member with that ID exists",
                id
            );
            return;
        }

        if (member.Avatar == null)
        {
            _logger.Warning("Clear avatar job queued for {MemberId} with null avatar", id);
            return;
        }

        await objectStorageService.RemoveObjectAsync(Path(member.Id, member.Avatar));

        member.Avatar = null;
        await db.SaveChangesAsync();
    }

    public static string Path(Snowflake id, string hash) => $"members/{id}/avatars/{hash}.webp";
}
