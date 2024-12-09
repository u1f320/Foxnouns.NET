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
        DataExport? export = await db
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
