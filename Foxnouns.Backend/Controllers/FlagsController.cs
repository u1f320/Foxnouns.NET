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
using Coravel.Queuing.Interfaces;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XidNet;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users/@me/flags")]
public class FlagsController(
    DatabaseContext db,
    UserRendererService userRenderer,
    ISnowflakeGenerator snowflakeGenerator,
    IQueue queue
) : ApiControllerBase
{
    [HttpGet]
    [Limit(UsableByDeletedUsers = true)]
    [Authorize("user.read_flags")]
    [ProducesResponseType<IEnumerable<PrideFlagResponse>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFlagsAsync(CancellationToken ct = default)
    {
        List<PrideFlag> flags = await db
            .PrideFlags.Where(f => f.UserId == CurrentUser!.Id)
            .OrderBy(f => f.Name)
            .ThenBy(f => f.Id)
            .ToListAsync(ct);

        return Ok(flags.Select(userRenderer.RenderPrideFlag));
    }

    public const int MaxFlagCount = 500;

    [HttpPost]
    [Authorize("user.update_flags")]
    [ProducesResponseType<PrideFlagResponse>(statusCode: StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CreateFlagAsync([FromBody] CreateFlagRequest req)
    {
        int flagCount = await db.PrideFlags.Where(f => f.UserId == CurrentUser!.Id).CountAsync();
        if (flagCount >= MaxFlagCount)
            throw new ApiError.BadRequest("Maximum number of flags reached");

        ValidationUtils.Validate(ValidateFlag(req.Name, req.Description, req.Image));

        var flag = new PrideFlag
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            LegacyId = Xid.NewXid().ToString(),
            UserId = CurrentUser!.Id,
            Name = req.Name,
            Description = req.Description,
        };

        db.Add(flag);
        await db.SaveChangesAsync();

        queue.QueueInvocableWithPayload<CreateFlagInvocable, CreateFlagPayload>(
            new CreateFlagPayload(flag.Id, CurrentUser!.Id, req.Image)
        );

        return Accepted(userRenderer.RenderPrideFlag(flag));
    }

    [HttpPatch("{id}")]
    [Authorize("user.create_flags")]
    [ProducesResponseType<PrideFlagResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateFlagAsync(Snowflake id, [FromBody] UpdateFlagRequest req)
    {
        ValidationUtils.Validate(ValidateFlag(req.Name, req.Description, null));

        PrideFlag? flag = await db.PrideFlags.FirstOrDefaultAsync(f =>
            f.Id == id && f.UserId == CurrentUser!.Id
        );
        if (flag == null)
            throw new ApiError.NotFound("Unknown flag ID, or it's not your flag.");

        if (req.Name != null)
            flag.Name = req.Name;

        if (req.HasProperty(nameof(req.Description)))
            flag.Description = req.Description;

        db.Update(flag);
        await db.SaveChangesAsync();

        return Ok(userRenderer.RenderPrideFlag(flag));
    }

    [HttpDelete("{id}")]
    [Authorize("user.update_flags")]
    public async Task<IActionResult> DeleteFlagAsync(Snowflake id)
    {
        PrideFlag? flag = await db.PrideFlags.FirstOrDefaultAsync(f =>
            f.Id == id && f.UserId == CurrentUser!.Id
        );
        if (flag == null)
            throw new ApiError.NotFound("Unknown flag ID, or it's not your flag.");

        db.PrideFlags.Remove(flag);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static List<(string, ValidationError?)> ValidateFlag(
        string? name,
        string? description,
        string? imageData
    )
    {
        var errors = new List<(string, ValidationError?)>();

        if (name != null)
        {
            switch (name.Length)
            {
                case < 1:
                    errors.Add(
                        (
                            "name",
                            ValidationError.LengthError("Name is too short", 1, 100, name.Length)
                        )
                    );
                    break;
                case > 100:
                    errors.Add(
                        (
                            "name",
                            ValidationError.LengthError("Name is too long", 1, 100, name.Length)
                        )
                    );
                    break;
            }
        }

        if (description != null)
        {
            switch (description.Length)
            {
                case < 1:
                    errors.Add(
                        (
                            "description",
                            ValidationError.LengthError(
                                "Description is too short",
                                1,
                                100,
                                description.Length
                            )
                        )
                    );
                    break;
                case > 500:
                    errors.Add(
                        (
                            "description",
                            ValidationError.LengthError(
                                "Description is too long",
                                1,
                                100,
                                description.Length
                            )
                        )
                    );
                    break;
            }
        }

        if (imageData != null)
        {
            switch (imageData.Length)
            {
                case 0:
                    errors.Add(
                        (
                            "image",
                            ValidationError.GenericValidationError("Image cannot be empty", null)
                        )
                    );
                    break;
                case > 1_500_000:
                    errors.Add(
                        (
                            "image",
                            ValidationError.GenericValidationError("Image is too large", null)
                        )
                    );
                    break;
            }
        }

        return errors;
    }
}
