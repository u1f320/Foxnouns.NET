using System.Security.Cryptography;
using Coravel.Invocable;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;

namespace Foxnouns.Backend.Jobs;

public class MemberAvatarUpdateInvocable(DatabaseContext db, ObjectStorageService objectStorageService, ILogger logger)
    : IInvocable, IInvocableWithPayload<AvatarUpdatePayload>
{
    private readonly ILogger _logger = logger.ForContext<UserAvatarUpdateInvocable>();
    public required AvatarUpdatePayload Payload { get; set; }

    public async Task Invoke()
    {
        if (Payload.NewAvatar != null) await UpdateMemberAvatarAsync(Payload.Id, Payload.NewAvatar);
        else await ClearMemberAvatarAsync(Payload.Id);
    }

    private async Task UpdateMemberAvatarAsync(Snowflake id, string newAvatar)
    {
        _logger.Debug("Updating avatar for member {MemberId}", id);

        var member = await db.Members.FindAsync(id);
        if (member == null)
        {
            _logger.Warning("Update avatar job queued for {MemberId} but no member with that ID exists", id);
            return;
        }

        try
        {
            var (hash, image) = await newAvatar.ConvertBase64UriToImage(size: 512, crop: true);
            var prevHash = member.Avatar;

            await objectStorageService.PutObjectAsync(Path(id, hash), image, "image/webp");

            member.Avatar = hash;
            await db.SaveChangesAsync();

            if (prevHash != null && prevHash != hash)
                await objectStorageService.RemoveObjectAsync(Path(id, prevHash));

            _logger.Information("Updated avatar for member {MemberId}", id);
        }
        catch (ArgumentException ae)
        {
            _logger.Warning("Invalid data URI for new avatar for member {MemberId}: {Reason}", id, ae.Message);
        }
    }

    private async Task ClearMemberAvatarAsync(Snowflake id)
    {
        _logger.Debug("Clearing avatar for member {MemberId}", id);

        var member = await db.Members.FindAsync(id);
        if (member == null)
        {
            _logger.Warning("Clear avatar job queued for {MemberId} but no member with that ID exists", id);
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