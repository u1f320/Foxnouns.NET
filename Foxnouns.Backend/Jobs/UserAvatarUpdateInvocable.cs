using System.Security.Cryptography;
using Coravel.Invocable;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;

namespace Foxnouns.Backend.Jobs;

public class UserAvatarUpdateInvocable(DatabaseContext db, ObjectStorageService objectStorageService, ILogger logger)
    : IInvocable, IInvocableWithPayload<AvatarUpdatePayload>
{
    private readonly ILogger _logger = logger.ForContext<UserAvatarUpdateInvocable>();
    public required AvatarUpdatePayload Payload { get; set; }

    public async Task Invoke()
    {
        if (Payload.NewAvatar != null) await UpdateUserAvatarAsync(Payload.Id, Payload.NewAvatar);
        else await ClearUserAvatarAsync(Payload.Id);
    }

    private async Task UpdateUserAvatarAsync(Snowflake id, string newAvatar)
    {
        _logger.Debug("Updating avatar for user {MemberId}", id);

        var user = await db.Users.FindAsync(id);
        if (user == null)
        {
            _logger.Warning("Update avatar job queued for {UserId} but no user with that ID exists", id);
            return;
        }

        try
        {
            var (hash, image) = await newAvatar.ConvertBase64UriToImage(size: 512, crop: true);
            image.Seek(0, SeekOrigin.Begin);
            var prevHash = user.Avatar;

            await objectStorageService.PutObjectAsync(Path(id, hash), image, "image/webp");

            user.Avatar = hash;
            await db.SaveChangesAsync();

            if (prevHash != null && prevHash != hash)
                await objectStorageService.RemoveObjectAsync(Path(id, prevHash));

            _logger.Information("Updated avatar for user {UserId}", id);
        }
        catch (ArgumentException ae)
        {
            _logger.Warning("Invalid data URI for new avatar for user {UserId}: {Reason}", id, ae.Message);
        }
    }

    private async Task ClearUserAvatarAsync(Snowflake id)
    {
        _logger.Debug("Clearing avatar for user {MemberId}", id);

        var user = await db.Users.FindAsync(id);
        if (user == null)
        {
            _logger.Warning("Clear avatar job queued for {UserId} but no user with that ID exists", id);
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