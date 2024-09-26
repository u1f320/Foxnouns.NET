using Foxnouns.Backend.Database;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users/@me/flags")]
public class FlagsController(DatabaseContext db, UserRendererService userRenderer) : ApiControllerBase
{
    [HttpGet]
    [Authorize("identify")]
    [ProducesResponseType<IEnumerable<PrideFlagResponse>>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFlagsAsync(CancellationToken ct = default)
    {
        var flags = await db.PrideFlags.Where(f => f.UserId == CurrentUser!.Id).ToListAsync(ct);

        return Ok(flags.Select(f => new PrideFlagResponse(
            f.Id, userRenderer.ImageUrlFor(f), f.Name, f.Description)));
    }

    private record PrideFlagResponse(
        Snowflake Id,
        string ImageUrl,
        string Name,
        string? Description);
}