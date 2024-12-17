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
using System.Net;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers.Moderation;

[Route("/api/v2/moderation")]
[Authorize("user.moderation")]
[Limit(RequireModerator = true)]
public class ModActionsController(
    DatabaseContext db,
    ModerationService moderationService,
    ModerationRendererService moderationRenderer
) : ApiControllerBase
{
    [HttpPost("warnings/{id}")]
    public async Task<IActionResult> WarnUserAsync(Snowflake id, [FromBody] WarnUserRequest req)
    {
        User user = await db.ResolveUserAsync(id);
        if (user.Deleted)
        {
            throw new ApiError(
                "This user is already deleted.",
                HttpStatusCode.BadRequest,
                ErrorCode.InvalidWarningTarget
            );
        }

        if (user.Id == CurrentUser!.Id)
        {
            throw new ApiError(
                "You can't warn yourself.",
                HttpStatusCode.BadRequest,
                ErrorCode.InvalidWarningTarget
            );
        }

        Member? member = null;
        if (req.MemberId != null)
        {
            member = await db.Members.FirstOrDefaultAsync(m =>
                m.Id == req.MemberId && m.UserId == user.Id
            );
            if (member == null)
                throw new ApiError.NotFound("No member with that ID found.");
        }

        Report? report = null;
        if (req.ReportId != null)
        {
            report = await db.Reports.FindAsync(req.ReportId);
            if (report is not { Status: ReportStatus.Open })
            {
                throw new ApiError.NotFound(
                    "No report with that ID found, or it's already closed."
                );
            }
        }

        AuditLogEntry entry = await moderationService.ExecuteWarningAsync(
            CurrentUser,
            user,
            member,
            report,
            req.Reason,
            req.ClearFields
        );

        return Ok(moderationRenderer.RenderAuditLogEntry(entry));
    }

    [HttpPost("suspensions/{id}")]
    public async Task<IActionResult> SuspendUserAsync(
        Snowflake id,
        [FromBody] SuspendUserRequest req
    )
    {
        User user = await db.ResolveUserAsync(id);
        if (user.Deleted)
        {
            throw new ApiError(
                "This user is already deleted.",
                HttpStatusCode.BadRequest,
                ErrorCode.InvalidWarningTarget
            );
        }

        if (user.Id == CurrentUser!.Id)
        {
            throw new ApiError(
                "You can't warn yourself.",
                HttpStatusCode.BadRequest,
                ErrorCode.InvalidWarningTarget
            );
        }

        Report? report = null;
        if (req.ReportId != null)
        {
            report = await db.Reports.FindAsync(req.ReportId);
            if (report is not { Status: ReportStatus.Open })
            {
                throw new ApiError.NotFound(
                    "No report with that ID found, or it's already closed."
                );
            }
        }

        AuditLogEntry entry = await moderationService.ExecuteSuspensionAsync(
            CurrentUser,
            user,
            report,
            req.Reason,
            req.ClearProfile
        );

        return Ok(moderationRenderer.RenderAuditLogEntry(entry));
    }
}
