using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Foxnouns.Backend.Services;

public class ObjectStorageService(ILogger logger, Config config, IMinioClient minio)
{
    private readonly ILogger _logger = logger.ForContext<ObjectStorageService>();
    
    public async Task RemoveObjectAsync(string path)
    {
        logger.Debug("Deleting object at path {Path}", path);
        try
        {
            await minio.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(config.Storage.Bucket).WithObject(path));
        }
        catch (InvalidObjectNameException)
        {
        }
    }

    public async Task PutObjectAsync(string path, Stream data, string contentType)
    {
        await minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(config.Storage.Bucket)
            .WithObject(path)
            .WithObjectSize(data.Length)
            .WithStreamData(data)
            .WithContentType(contentType)
        );
    }
}