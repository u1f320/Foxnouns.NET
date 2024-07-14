using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Utils;
using Hangfire;
using Minio;
using Minio.DataModel.Args;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Foxnouns.Backend.Jobs;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Hangfire jobs need to be public")]
public class AvatarUpdateJob(DatabaseContext db, IMinioClient minio, Config config, ILogger logger)
{
    private readonly string[] _validContentTypes = ["image/png", "image/webp", "image/jpeg"];

    public static void QueueUpdateUserAvatar(Snowflake id, string? newAvatar)
    {
        if (newAvatar != null)
            BackgroundJob.Enqueue<AvatarUpdateJob>(job => job.UpdateUserAvatar(id, newAvatar));
        else
            BackgroundJob.Enqueue<AvatarUpdateJob>(job => job.ClearUserAvatar(id));
    }

    public static void QueueUpdateMemberAvatar(Snowflake id, string? newAvatar)
    {
        if (newAvatar != null)
            BackgroundJob.Enqueue<AvatarUpdateJob>(job => job.UpdateMemberAvatar(id, newAvatar));
        else
            BackgroundJob.Enqueue<AvatarUpdateJob>(job => job.ClearMemberAvatar(id));
    }

    public async Task UpdateUserAvatar(Snowflake id, string newAvatar)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null)
        {
            logger.Warning("Update avatar job queued for {UserId} but no user with that ID exists", id);
            return;
        }

        try
        {
            var image = await ConvertAvatar(newAvatar);
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(image)).ToLower();
            image.Seek(0, SeekOrigin.Begin);
            var prevHash = user.Avatar;

            await minio.PutObjectAsync(new PutObjectArgs()
                .WithBucket(config.Storage.Bucket)
                .WithObject(UserAvatarPath(id, hash))
                .WithObjectSize(image.Length)
                .WithStreamData(image)
                .WithContentType("image/webp")
            );

            user.Avatar = hash;
            await db.SaveChangesAsync();

            if (prevHash != null && prevHash != hash)
                await minio.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(config.Storage.Bucket)
                    .WithObject(UserAvatarPath(id, prevHash))
                );

            logger.Information("Updated avatar for user {UserId}", id);
        }
        catch (ArgumentException ae)
        {
            logger.Warning("Invalid data URI for new avatar for user {UserId}: {Reason}", id, ae.Message);
        }
    }

    public async Task ClearUserAvatar(Snowflake id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null)
        {
            logger.Warning("Clear avatar job queued for {UserId} but no user with that ID exists", id);
            return;
        }

        if (user.Avatar == null)
        {
            logger.Warning("Clear avatar job queued for {UserId} with null avatar", id);
            return;
        }

        await minio.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(config.Storage.Bucket)
            .WithObject(UserAvatarPath(user.Id, user.Avatar))
        );

        user.Avatar = null;
        await db.SaveChangesAsync();
    }

    public async Task UpdateMemberAvatar(Snowflake id, string newAvatar)
    {
        var member = await db.Members.FindAsync(id);
        if (member == null)
        {
            logger.Warning("Update avatar job queued for {MemberId} but no member with that ID exists", id);
            return;
        }

        try
        {
            var image = await ConvertAvatar(newAvatar);
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(image)).ToLower();
            image.Seek(0, SeekOrigin.Begin);
            var prevHash = member.Avatar;

            await minio.PutObjectAsync(new PutObjectArgs()
                .WithBucket(config.Storage.Bucket)
                .WithObject(MemberAvatarPath(id, hash))
                .WithObjectSize(image.Length)
                .WithStreamData(image)
                .WithContentType("image/webp")
            );

            member.Avatar = hash;
            await db.SaveChangesAsync();

            if (prevHash != null && prevHash != hash)
                await minio.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(config.Storage.Bucket)
                    .WithObject(MemberAvatarPath(id, prevHash))
                );

            logger.Information("Updated avatar for member {MemberId}", id);
        }
        catch (ArgumentException ae)
        {
            logger.Warning("Invalid data URI for new avatar for member {MemberId}: {Reason}", id, ae.Message);
        }
    }

    public async Task ClearMemberAvatar(Snowflake id)
    {
        var member = await db.Members.FindAsync(id);
        if (member == null)
        {
            logger.Warning("Clear avatar job queued for {MemberId} but no member with that ID exists", id);
            return;
        }

        if (member.Avatar == null)
        {
            logger.Warning("Clear avatar job queued for {MemberId} with null avatar", id);
            return;
        }

        await minio.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(config.Storage.Bucket)
            .WithObject(MemberAvatarPath(member.Id, member.Avatar))
        );

        member.Avatar = null;
        await db.SaveChangesAsync();
    }

    private async Task<Stream> ConvertAvatar(string uri)
    {
        if (!uri.StartsWith("data:image/"))
            throw new ArgumentException("Not a data URI", nameof(uri));

        var split = uri.Remove(0, "data:".Length).Split(";base64,");
        var contentType = split[0];
        var encoded = split[1];
        if (!_validContentTypes.Contains(contentType))
            throw new ArgumentException("Invalid content type for image", nameof(uri));

        if (!AuthUtils.TryFromBase64String(encoded, out var rawImage))
            throw new ArgumentException("Invalid base64 string", nameof(uri));

        var image = Image.Load(rawImage);

        var processor = new ResizeProcessor(
            new ResizeOptions { Size = new Size(512), Mode = ResizeMode.Crop, Position = AnchorPositionMode.Center },
            image.Size
        );

        image.Mutate(x => x.ApplyProcessor(processor));

        var stream = new MemoryStream(64 * 1024);
        await image.SaveAsync(stream, new WebpEncoder { Quality = 95, NearLossless = false });
        return stream;
    }

    private static string UserAvatarPath(Snowflake id, string hash) => $"users/{id}/avatars/{hash}.webp";
    private static string MemberAvatarPath(Snowflake id, string hash) => $"members/{id}/avatars/{hash}.webp";
}