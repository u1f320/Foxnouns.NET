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
using Coravel.Invocable;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;

namespace Foxnouns.Backend.Jobs;

public class UserAvatarUpdateInvocable(
    DatabaseContext db,
    ObjectStorageService objectStorageService,
    ILogger logger
) : IInvocable, IInvocableWithPayload<AvatarUpdatePayload>
{
    private readonly ILogger _logger = logger.ForContext<UserAvatarUpdateInvocable>();
    public required AvatarUpdatePayload Payload { get; set; }

    public async Task Invoke()
    {
        if (Payload.NewAvatar != null)
            await UpdateUserAvatarAsync(Payload.Id, Payload.NewAvatar);
        else
            await ClearUserAvatarAsync(Payload.Id);
    }

    private async Task UpdateUserAvatarAsync(Snowflake id, string newAvatar)
    {
        _logger.Debug("Updating avatar for user {MemberId}", id);

        User? user = await db.Users.FindAsync(id);
        if (user == null)
        {
            _logger.Warning(
                "Update avatar job queued for {UserId} but no user with that ID exists",
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
            image.Seek(0, SeekOrigin.Begin);
            string? prevHash = user.Avatar;

            await objectStorageService.PutObjectAsync(Path(id, hash), image, "image/webp");

            user.Avatar = hash;
            await db.SaveChangesAsync();

            if (prevHash != null && prevHash != hash)
                await objectStorageService.RemoveObjectAsync(Path(id, prevHash));

            _logger.Information("Updated avatar for user {UserId}", id);
        }
        catch (ArgumentException ae)
        {
            _logger.Warning(
                "Invalid data URI for new avatar for user {UserId}: {Reason}",
                id,
                ae.Message
            );
        }
    }

    private async Task ClearUserAvatarAsync(Snowflake id)
    {
        _logger.Debug("Clearing avatar for user {MemberId}", id);

        User? user = await db.Users.FindAsync(id);
        if (user == null)
        {
            _logger.Warning(
                "Clear avatar job queued for {UserId} but no user with that ID exists",
                id
            );
            return;
        }

        if (user.Avatar == null)
        {
            _logger.Warning("Clear avatar job queued for {UserId} with null avatar", id);
            return;
        }

        await objectStorageService.RemoveObjectAsync(Path(user.Id, user.Avatar));

        user.Avatar = null;
        await db.SaveChangesAsync();
    }

    public static string Path(Snowflake id, string hash) => $"users/{id}/avatars/{hash}.webp";
}
