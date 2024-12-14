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
}
