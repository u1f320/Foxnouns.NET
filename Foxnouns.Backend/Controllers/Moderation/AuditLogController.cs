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
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers.Moderation;

[Route("/api/v2/moderation/audit-log")]
[Authorize("user.moderation")]
[Limit(RequireModerator = true)]
public class AuditLogController(DatabaseContext db, ModerationRendererService moderationRenderer)
    : ApiControllerBase
{
    public async Task<IActionResult> GetAuditLogAsync(
        [FromQuery] AuditLogEntryType? type = null,
        [FromQuery] int? limit = null,
        [FromQuery] Snowflake? before = null,
        [FromQuery] Snowflake? after = null,
        [FromQuery(Name = "by-moderator")] Snowflake? byModerator = null
    )
    {
        limit = limit switch
        {
            > 100 => 100,
            < 0 => 100,
            null => 100,
            _ => limit,
        };

        IQueryable<AuditLogEntry> query = db.AuditLog.OrderByDescending(e => e.Id);

        if (before != null)
            query = query.Where(e => e.Id < before.Value);
        else if (after != null)
            query = query.Where(e => e.Id > after.Value);

        if (type != null)
            query = query.Where(e => e.Type == type);
        if (byModerator != null)
            query = query.Where(e => e.ModeratorId == byModerator.Value);

        List<AuditLogEntry> entries = await query.Take(limit!.Value).ToListAsync();

        return Ok(entries.Select(moderationRenderer.RenderAuditLogEntry));
    }

    [HttpGet("moderators")]
    public async Task<IActionResult> GetModeratorsAsync(CancellationToken ct = default)
    {
        var moderators = await db
            .Users.Where(u =>
                !u.Deleted && (u.Role == UserRole.Admin || u.Role == UserRole.Moderator)
            )
            .Select(u => new { u.Id, u.Username })
            .OrderBy(u => u.Id)
            .ToListAsync(ct);

        return Ok(moderators);
    }
}
