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
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Jobs;

public class CreateFlagJob(
    DatabaseContext db,
    ObjectStorageService objectStorageService,
    ILogger logger
)
{
    private readonly ILogger _logger = logger.ForContext<CreateFlagJob>();

    public static void Enqueue(CreateFlagPayload payload)
    {
        BackgroundJob.Enqueue<CreateFlagJob>(j => j.InvokeAsync(payload));
    }

    public async Task InvokeAsync(CreateFlagPayload payload)
    {
        _logger.Information(
            "Creating flag {FlagId} for user {UserId} with image data length {DataLength}",
            payload.Id,
            payload.UserId,
            payload.ImageData.Length
        );

        try
        {
            PrideFlag? flag = await db.PrideFlags.FirstOrDefaultAsync(f =>
                f.Id == payload.Id && f.UserId == payload.UserId
            );
            if (flag == null)
            {
                _logger.Warning(
                    "Got a flag create job for {FlagId} but it doesn't exist, aborting",
                    payload.Id
                );
                return;
            }

            (string? hash, Stream? image) = await ImageObjectExtensions.ConvertBase64UriToImage(
                payload.ImageData,
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
            _logger.Warning("Invalid data URI for flag {FlagId}: {Reason}", payload.Id, ae.Message);
        }

        throw new NotImplementedException();
    }

    public static string Path(string hash) => $"flags/{hash}.webp";
}
