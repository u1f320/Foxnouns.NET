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
using System.Security.Cryptography;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Foxnouns.Backend.Extensions;

public static class ImageObjectExtensions
{
    private static readonly string[] ValidContentTypes = ["image/png", "image/webp", "image/jpeg"];

    public static async Task DeleteMemberAvatarAsync(
        this ObjectStorageService objectStorageService,
        Snowflake id,
        string hash,
        CancellationToken ct = default
    ) =>
        await objectStorageService.RemoveObjectAsync(
            MemberAvatarUpdateInvocable.Path(id, hash),
            ct
        );

    public static async Task DeleteUserAvatarAsync(
        this ObjectStorageService objectStorageService,
        Snowflake id,
        string hash,
        CancellationToken ct = default
    ) => await objectStorageService.RemoveObjectAsync(UserAvatarUpdateInvocable.Path(id, hash), ct);

    public static async Task DeleteFlagAsync(
        this ObjectStorageService objectStorageService,
        string hash,
        CancellationToken ct = default
    ) => await objectStorageService.RemoveObjectAsync(CreateFlagInvocable.Path(hash), ct);

    public static async Task<(string Hash, Stream Image)> ConvertBase64UriToImage(
        string uri,
        int size,
        bool crop
    )
    {
        if (!uri.StartsWith("data:image/"))
            throw new ArgumentException("Not a data URI", nameof(uri));

        string[] split = uri.Remove(0, "data:".Length).Split(";base64,");
        string contentType = split[0];
        string encoded = split[1];
        if (!ValidContentTypes.Contains(contentType))
            throw new ArgumentException("Invalid content type for image", nameof(uri));

        if (!AuthUtils.TryFromBase64String(encoded, out byte[]? rawImage))
            throw new ArgumentException("Invalid base64 string", nameof(uri));

        var image = Image.Load(rawImage);

        var processor = new ResizeProcessor(
            new ResizeOptions
            {
                Size = new Size(size),
                Mode = crop ? ResizeMode.Crop : ResizeMode.Max,
                Position = AnchorPositionMode.Center,
            },
            image.Size
        );

        image.Mutate(x => x.ApplyProcessor(processor));

        var stream = new MemoryStream(64 * 1024);
        await image.SaveAsync(stream, new WebpEncoder { Quality = 95, NearLossless = false });

        stream.Seek(0, SeekOrigin.Begin);
        string hash = Convert.ToHexString(await SHA256.HashDataAsync(stream)).ToLower();
        stream.Seek(0, SeekOrigin.Begin);

        return (hash, stream);
    }
}
