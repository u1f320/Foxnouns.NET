using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Controllers.Moderation;

[Route("/api/v2/notices")]
[Authorize("user.moderation")]
[Limit(RequireModerator = true)]
public class NoticesController(
    DatabaseContext db,
    UserRendererService userRenderer,
    ISnowflakeGenerator snowflakeGenerator,
    IClock clock
) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNoticesAsync(CancellationToken ct = default)
    {
        List<Notice> notices = await db
            .Notices.Include(n => n.Author)
            .OrderByDescending(n => n.Id)
            .ToListAsync(ct);
        return Ok(notices.Select(RenderNotice));
    }

    [HttpPost]
    public async Task<IActionResult> CreateNoticeAsync(CreateNoticeRequest req)
    {
        Instant now = clock.GetCurrentInstant();
        if (req.StartTime < now)
        {
            throw new ApiError.BadRequest(
                "Start time cannot be in the past",
                "start_time",
                req.StartTime
            );
        }

        if (req.EndTime < now)
        {
            throw new ApiError.BadRequest(
                "End time cannot be in the past",
                "end_time",
                req.EndTime
            );
        }

        var notice = new Notice
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            Message = req.Message,
            StartTime = req.StartTime ?? clock.GetCurrentInstant(),
            EndTime = req.EndTime,
            Author = CurrentUser!,
        };

        db.Add(notice);
        await db.SaveChangesAsync();

        return Ok(RenderNotice(notice));
    }

    private NoticeResponse RenderNotice(Notice notice) =>
        new(
            notice.Id,
            notice.Message,
            notice.StartTime,
            notice.EndTime,
            userRenderer.RenderPartialUser(notice.Author)
        );
}
