using Coravel.Queuing.Interfaces;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Controllers;

[Route("/api/internal/data-exports")]
[Authorize("identify")]
public class ExportsController(
    ILogger logger,
    Config config,
    IClock clock,
    DatabaseContext db,
    IQueue queue
) : ApiControllerBase
{
    private static readonly Duration MinimumTimeBetween = Duration.FromDays(1);
    private readonly ILogger _logger = logger.ForContext<ExportsController>();

    [HttpGet]
    public async Task<IActionResult> GetDataExportsAsync()
    {
        var export = await db
            .DataExports.Where(d => d.UserId == CurrentUser!.Id)
            .OrderByDescending(d => d.Id)
            .FirstOrDefaultAsync();
        if (export == null)
            return Ok(new DataExportResponse(null, null));

        return Ok(
            new DataExportResponse(
                ExportUrl(CurrentUser!.Id, export.Filename),
                export.Id.Time + DataExport.Expiration
            )
        );
    }

    private string ExportUrl(Snowflake userId, string filename) =>
        $"{config.MediaBaseUrl}/data-exports/{userId}/{filename}.zip";

    private record DataExportResponse(string? Url, Instant? ExpiresAt);

    [HttpPost]
    public async Task<IActionResult> QueueDataExportAsync()
    {
        var snowflakeToCheck = Snowflake.FromInstant(
            clock.GetCurrentInstant() - MinimumTimeBetween
        );
        _logger.Debug(
            "Checking if user {UserId} has data exports newer than {Snowflake}",
            CurrentUser!.Id,
            snowflakeToCheck
        );
        if (
            await db.DataExports.AnyAsync(d =>
                d.UserId == CurrentUser.Id && d.Id > snowflakeToCheck
            )
        )
        {
            throw new ApiError.BadRequest("You can't request a new data export so soon.");
        }

        queue.QueueInvocableWithPayload<CreateDataExportInvocable, CreateDataExportPayload>(
            new CreateDataExportPayload(CurrentUser.Id)
        );

        return NoContent();
    }
}
