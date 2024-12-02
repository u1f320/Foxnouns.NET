using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Foxnouns.Backend.Services;

public class ObjectStorageService(ILogger logger, Config config, IMinioClient minioClient)
{
    private readonly ILogger _logger = logger.ForContext<ObjectStorageService>();

    public async Task RemoveObjectAsync(string path, CancellationToken ct = default)
    {
        _logger.Debug("Deleting object at path {Path}", path);
        try
        {
            await minioClient.RemoveObjectAsync(
                new RemoveObjectArgs().WithBucket(config.Storage.Bucket).WithObject(path),
                ct
            );
        }
        catch (InvalidObjectNameException)
        {
            // ignore non-existent objects
        }
    }

    public async Task PutObjectAsync(
        string path,
        Stream data,
        string contentType,
        CancellationToken ct = default
    )
    {
        _logger.Debug(
            "Putting object at path {Path} with length {Length} and content type {ContentType}",
            path,
            data.Length,
            contentType
        );

        await minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(config.Storage.Bucket)
                .WithObject(path)
                .WithObjectSize(data.Length)
                .WithStreamData(data)
                .WithContentType(contentType),
            ct
        );
    }

    public async Task GetObjectAsync(string path, CancellationToken ct = default)
    {
        var stream = new MemoryStream();
        var resp = await minioClient.GetObjectAsync(
            new GetObjectArgs().WithBucket(config.Storage.Bucket).WithObject(path),
            ct
        );
    }
}
