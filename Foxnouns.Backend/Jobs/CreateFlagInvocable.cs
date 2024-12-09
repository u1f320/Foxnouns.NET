using Coravel.Invocable;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Jobs;

public class CreateFlagInvocable(
    DatabaseContext db,
    ObjectStorageService objectStorageService,
    ILogger logger
) : IInvocable, IInvocableWithPayload<CreateFlagPayload>
{
    private readonly ILogger _logger = logger.ForContext<CreateFlagInvocable>();
    public required CreateFlagPayload Payload { get; set; }

    public async Task Invoke()
    {
        _logger.Information(
            "Creating flag {FlagId} for user {UserId} with image data length {DataLength}",
            Payload.Id,
            Payload.UserId,
            Payload.ImageData.Length
        );

        try
        {
            PrideFlag? flag = await db.PrideFlags.FirstOrDefaultAsync(f =>
                f.Id == Payload.Id && f.UserId == Payload.UserId
            );
            if (flag == null)
            {
                _logger.Warning(
                    "Got a flag create job for {FlagId} but it doesn't exist, aborting",
                    Payload.Id
                );
                return;
            }

            (string? hash, Stream? image) = await ImageObjectExtensions.ConvertBase64UriToImage(
                Payload.ImageData,
                256,
                false
            );
            await objectStorageService.PutObjectAsync(Path(hash), image, "image/webp");

            flag.Hash = hash;
            db.Update(flag);
            await db.SaveChangesAsync();

            _logger.Information("Uploaded flag {FlagId} with hash {Hash}", flag.Id, flag.Hash);
        }
        catch (ArgumentException ae)
        {
            _logger.Warning("Invalid data URI for flag {FlagId}: {Reason}", Payload.Id, ae.Message);
        }

        throw new NotImplementedException();
    }

    public static string Path(string hash) => $"flags/{hash}.webp";
}
