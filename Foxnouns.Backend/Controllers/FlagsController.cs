using Coravel.Queuing.Interfaces;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
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
    IQueue queue) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<FlagsController>();
    
    [HttpGet]
    [Authorize("identify")]
    [ProducesResponseType<IEnumerable<PrideFlagResponse>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFlagsAsync(CancellationToken ct = default)
    {
        var flags = await db.PrideFlags.Where(f => f.UserId == CurrentUser!.Id).ToListAsync(ct);

        return Ok(flags.Select(ToResponse));
    }

    [HttpPost]
    [Authorize("user.update")]
    [ProducesResponseType<PrideFlagResponse>(statusCode: StatusCodes.Status202Accepted)]
    public IActionResult CreateFlag([FromBody] CreateFlagRequest req)
    {
        var id = snowflakeGenerator.GenerateSnowflake();

        queue.QueueInvocableWithPayload<CreateFlagInvocable, CreateFlagPayload>(
            new CreateFlagPayload(id, CurrentUser!.Id, req.Name, req.Image, req.Description));

        return Accepted(new CreateFlagResponse(id, req.Name, req.Description));
    }

    public record CreateFlagRequest(string Name, string Image, string? Description);
    private record CreateFlagResponse(Snowflake Id, string Name, string? Description);

    [HttpDelete("{id}")]
    [Authorize("user.update")]
    public async Task<IActionResult> DeleteFlagAsync(Snowflake id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        
        var flag = await db.PrideFlags.FirstOrDefaultAsync(f => f.Id == id && f.UserId == CurrentUser!.Id);
        if (flag == null) throw new ApiError.NotFound("Unknown flag ID, or it's not your flag.");

        var hash = flag.Hash;

        db.PrideFlags.Remove(flag);
        await db.SaveChangesAsync();

        var flagCount = await db.PrideFlags.CountAsync(f => f.Hash == flag.Hash);
        if (flagCount == 0)
        {
            try
            {
                _logger.Information("Deleting flag file {Hash} as it is no longer used by any flags", hash);
                await objectStorageService.DeleteFlagAsync(hash);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error deleting flag file {Hash}", hash);
            }
        } else _logger.Debug("Flag file {Hash} is used by other flags, not deleting", hash);
        
        await tx.CommitAsync();

        return NoContent();
    }

    private PrideFlagResponse ToResponse(PrideFlag flag) =>
        new(flag.Id, userRenderer.ImageUrlFor(flag), flag.Name, flag.Description);

    private record PrideFlagResponse(
        Snowflake Id,
        string ImageUrl,
        string Name,
        string? Description);
}