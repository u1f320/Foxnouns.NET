using Coravel.Queuing.Interfaces;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users/@me/flags")]
public class FlagsController(
    ILogger logger,
    DatabaseContext db,
    UserRendererService userRenderer,
    ObjectStorageService objectStorageService,
    ISnowflakeGenerator snowflakeGenerator,
    IQueue queue
) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<FlagsController>();

    [HttpGet]
    [Authorize("identify")]
    [ProducesResponseType<IEnumerable<UserRendererService.PrideFlagResponse>>(
        statusCode: StatusCodes.Status200OK
    )]
    public async Task<IActionResult> GetFlagsAsync(CancellationToken ct = default)
    {
        var flags = await db.PrideFlags.Where(f => f.UserId == CurrentUser!.Id).ToListAsync(ct);

        return Ok(flags.Select(userRenderer.RenderPrideFlag));
    }

    [HttpPost]
    [Authorize("user.update")]
    [ProducesResponseType<UserRendererService.PrideFlagResponse>(
        statusCode: StatusCodes.Status202Accepted
    )]
    public IActionResult CreateFlag([FromBody] CreateFlagRequest req)
    {
        ValidationUtils.Validate(ValidateFlag(req.Name, req.Description, req.Image));

        var id = snowflakeGenerator.GenerateSnowflake();

        queue.QueueInvocableWithPayload<CreateFlagInvocable, CreateFlagPayload>(
            new CreateFlagPayload(id, CurrentUser!.Id, req.Name, req.Image, req.Description)
        );

        return Accepted(new CreateFlagResponse(id, req.Name, req.Description));
    }

    public record CreateFlagRequest(string Name, string Image, string? Description);

    private record CreateFlagResponse(Snowflake Id, string Name, string? Description);

    [HttpPatch("{id}")]
    [Authorize("user.update")]
    public async Task<IActionResult> UpdateFlagAsync(Snowflake id, [FromBody] UpdateFlagRequest req)
    {
        ValidationUtils.Validate(ValidateFlag(req.Name, req.Description, null));

        var flag = await db.PrideFlags.FirstOrDefaultAsync(f =>
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

    public class UpdateFlagRequest : PatchRequest
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
    }

    [HttpDelete("{id}")]
    [Authorize("user.update")]
    public async Task<IActionResult> DeleteFlagAsync(Snowflake id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var flag = await db.PrideFlags.FirstOrDefaultAsync(f =>
            f.Id == id && f.UserId == CurrentUser!.Id
        );
        if (flag == null)
            throw new ApiError.NotFound("Unknown flag ID, or it's not your flag.");

        var hash = flag.Hash;

        db.PrideFlags.Remove(flag);
        await db.SaveChangesAsync();

        var flagCount = await db.PrideFlags.CountAsync(f => f.Hash == flag.Hash);
        if (flagCount == 0)
        {
            try
            {
                _logger.Information(
                    "Deleting flag file {Hash} as it is no longer used by any flags",
                    hash
                );
                await objectStorageService.DeleteFlagAsync(hash);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error deleting flag file {Hash}", hash);
            }
        }
        else
            _logger.Debug("Flag file {Hash} is used by other flags, not deleting", hash);

        await tx.CommitAsync();

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
