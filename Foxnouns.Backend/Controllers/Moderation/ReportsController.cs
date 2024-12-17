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
using Newtonsoft.Json;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Moderation;

[Route("/api/v2/moderation")]
public class ReportsController(
    ILogger logger,
    DatabaseContext db,
    IClock clock,
    ISnowflakeGenerator snowflakeGenerator,
    UserRendererService userRenderer,
    MemberRendererService memberRenderer,
    ModerationRendererService moderationRenderer,
    ModerationService moderationService
) : ApiControllerBase
{
    private readonly ILogger _logger = logger.ForContext<ReportsController>();

    private Snowflake MaxReportId() =>
        Snowflake.FromInstant(clock.GetCurrentInstant() - Duration.FromHours(12));

    [HttpPost("report-user/{id}")]
    [Authorize("user.moderation")]
    public async Task<IActionResult> ReportUserAsync(
        Snowflake id,
        [FromBody] CreateReportRequest req
    )
    {
        User target = await db.ResolveUserAsync(id);

        if (target.Id == CurrentUser!.Id)
        {
            throw new ApiError(
                "You can't report yourself.",
                HttpStatusCode.BadRequest,
                ErrorCode.InvalidReportTarget
            );
        }

        Snowflake reportCutoff = MaxReportId();
        if (
            await db
                .Reports.Where(r =>
                    r.ReporterId == CurrentUser!.Id
                    && r.TargetUserId == target.Id
                    && r.Id > reportCutoff
                )
                .AnyAsync()
        )
        {
            _logger.Debug(
                "User {ReporterId} has already reported {TargetId} in the last 12 hours, ignoring report",
                CurrentUser!.Id,
                target.Id
            );
            return NoContent();
        }

        _logger.Information(
            "Creating report on {TargetId} by {ReporterId}",
            target.Id,
            CurrentUser!.Id
        );

        string snapshot = JsonConvert.SerializeObject(
            await userRenderer.RenderUserAsync(target, renderMembers: false)
        );

        var report = new Report
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            ReporterId = CurrentUser.Id,
            TargetUserId = target.Id,
            TargetMemberId = null,
            Reason = req.Reason,
            TargetType = ReportTargetType.User,
            TargetSnapshot = snapshot,
        };

        db.Reports.Add(report);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("report-member/{id}")]
    [Authorize("user.moderation")]
    public async Task<IActionResult> ReportMemberAsync(
        Snowflake id,
        [FromBody] CreateReportRequest req
    )
    {
        Member target = await db.ResolveMemberAsync(id);

        if (target.User.Id == CurrentUser!.Id)
        {
            throw new ApiError(
                "You can't report yourself.",
                HttpStatusCode.BadRequest,
                ErrorCode.InvalidReportTarget
            );
        }

        Snowflake reportCutoff = MaxReportId();
        if (
            await db
                .Reports.Where(r =>
                    r.ReporterId == CurrentUser!.Id
                    && r.TargetUserId == target.User.Id
                    && r.Id > reportCutoff
                )
                .AnyAsync()
        )
        {
            _logger.Debug(
                "User {ReporterId} has already reported {TargetId} in the last 12 hours, ignoring report",
                CurrentUser!.Id,
                target.User.Id
            );
            return NoContent();
        }

        _logger.Information(
            "Creating report on {TargetId} (member {TargetMemberId}) by {ReporterId}",
            target.User.Id,
            target.Id,
            CurrentUser!.Id
        );

        string snapshot = JsonConvert.SerializeObject(memberRenderer.RenderMember(target));

        var report = new Report
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            ReporterId = CurrentUser.Id,
            TargetUserId = target.User.Id,
            TargetMemberId = target.Id,
            Reason = req.Reason,
            TargetType = ReportTargetType.Member,
            TargetSnapshot = snapshot,
        };

        db.Reports.Add(report);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("reports")]
    [Authorize("user.moderation")]
    [Limit(RequireModerator = true)]
    public async Task<IActionResult> GetReportsAsync(
        [FromQuery] int? limit = null,
        [FromQuery] Snowflake? before = null,
        [FromQuery] Snowflake? after = null,
        [FromQuery(Name = "by-reporter")] Snowflake? byReporter = null,
        [FromQuery(Name = "by-target")] Snowflake? byTarget = null,
        [FromQuery(Name = "include-closed")] bool includeClosed = false
    )
    {
        limit = limit switch
        {
            > 100 => 100,
            < 0 => 100,
            null => 100,
            _ => limit,
        };

        IQueryable<Report> query = db
            .Reports.Include(r => r.Reporter)
            .Include(r => r.TargetUser)
            .Include(r => r.TargetMember);

        if (byTarget != null && await db.Users.AnyAsync(u => u.Id == byTarget.Value))
            query = query.Where(r => r.TargetUserId == byTarget.Value);

        if (byReporter != null && await db.Users.AnyAsync(u => u.Id == byReporter.Value))
            query = query.Where(r => r.ReporterId == byReporter.Value);

        if (before != null)
            query = query.Where(r => r.Id < before.Value).OrderByDescending(r => r.Id);
        else if (after != null)
            query = query.Where(r => r.Id > after.Value).OrderBy(r => r.Id);
        else
            query = query.OrderByDescending(r => r.Id);

        if (!includeClosed)
            query = query.Where(r => r.Status == ReportStatus.Open);

        List<Report> reports = await query.Take(limit!.Value).ToListAsync();

        return Ok(reports.Select(moderationRenderer.RenderReport));
    }

    [HttpPost("reports/{id}/ignore")]
    [Limit(RequireModerator = true)]
    public async Task<IActionResult> IgnoreReportAsync(
        Snowflake id,
        [FromBody] IgnoreReportRequest req
    )
    {
        Report? report = await db.Reports.FindAsync(id);
        if (report == null)
            throw new ApiError.NotFound("No report with that ID found.");
        if (report.Status != ReportStatus.Open)
            throw new ApiError.BadRequest("That report has already been handled.");

        AuditLogEntry entry = await moderationService.IgnoreReportAsync(
            CurrentUser!,
            report,
            req.Reason
        );

        return Ok(moderationRenderer.RenderAuditLogEntry(entry));
    }
}
