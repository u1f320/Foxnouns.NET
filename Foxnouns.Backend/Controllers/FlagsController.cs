using Coravel.Queuing.Interfaces;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users/@me/flags")]
public class FlagsController(
    DatabaseContext db,
    UserRendererService userRenderer,
    ISnowflakeGenerator snowflakeGenerator,
    IQueue queue) : ApiControllerBase
{
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

    public record CreateFlagResponse(Snowflake Id, string Name, string? Description);

    private PrideFlagResponse ToResponse(PrideFlag flag) =>
        new(flag.Id, userRenderer.ImageUrlFor(flag), flag.Name, flag.Description);

    private record PrideFlagResponse(
        Snowflake Id,
        string ImageUrl,
        string Name,
        string? Description);
}