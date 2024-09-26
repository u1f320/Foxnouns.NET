using Coravel.Invocable;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;

namespace Foxnouns.Backend.Jobs;

public class CreateFlagInvocable(DatabaseContext db, ObjectStorageService objectStorageService, ILogger logger)
    : IInvocable, IInvocableWithPayload<CreateFlagPayload>
{
    private readonly ILogger _logger = logger.ForContext<CreateFlagInvocable>();
    public required CreateFlagPayload Payload { get; set; }

    public async Task Invoke()
    {
        _logger.Information("Creating flag {FlagId} for user {UserId} with image data length {DataLength}", Payload.Id,
            Payload.UserId, Payload.ImageData.Length);

        try
        {
            var (hash, image) = await Payload.ImageData.ConvertBase64UriToImage(size: 256, crop: false);
            await objectStorageService.PutObjectAsync(Path(hash), image, "image/webp");

            var flag = new PrideFlag
            {
                Id = Payload.Id,
                UserId = Payload.UserId,
                Hash = hash,
                Name = Payload.Name,
                Description = Payload.Description
            };
            db.Add(flag);

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