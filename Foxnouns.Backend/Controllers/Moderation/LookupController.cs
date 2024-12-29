using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers.Moderation;

[Route("/api/v2/moderation/lookup")]
[Authorize("user.moderation")]
[Limit(RequireModerator = true)]
public class LookupController(
    DatabaseContext db,
    UserRendererService userRenderer,
    ModerationService moderationService,
    ModerationRendererService moderationRenderer
) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> QueryUsersAsync(
        [FromBody] QueryUsersRequest req,
        CancellationToken ct = default
    )
    {
        var query = db.Users.Select(u => new { u.Id, u.Username });
        query = req.Fuzzy
            ? query.Where(u => u.Username.Contains(req.Query))
            : query.Where(u => u.Username == req.Query);

        var users = await query.OrderBy(u => u.Id).Take(100).ToListAsync(ct);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> QueryUserAsync(Snowflake id, CancellationToken ct = default)
    {
        User user = await db.ResolveUserAsync(id, ct);

        bool showSensitiveData = await moderationService.ShowSensitiveDataAsync(
            CurrentUser!,
            user,
            ct
        );

        List<AuthMethod> authMethods = showSensitiveData
            ? await db
                .AuthMethods.Where(a => a.UserId == user.Id)
                .Include(a => a.FediverseApplication)
                .ToListAsync(ct)
            : [];

        return Ok(
            new QueryUserResponse(
                User: await userRenderer.RenderUserAsync(
                    user,
                    renderMembers: false,
                    renderAuthMethods: false,
                    ct: ct
                ),
                MemberListHidden: user.ListHidden,
                LastActive: user.LastActive,
                LastSidReroll: user.LastSidReroll,
                Suspended: user is { Deleted: true, DeletedBy: not null },
                Deleted: user.Deleted,
                ShowSensitiveData: showSensitiveData,
                AuthMethods: showSensitiveData
                    ? authMethods.Select(UserRendererService.RenderAuthMethod)
                    : null
            )
        );
    }

    [HttpPost("{id}/sensitive")]
    public async Task<IActionResult> QuerySensitiveUserDataAsync(
        Snowflake id,
        [FromBody] QuerySensitiveUserDataRequest req
    )
    {
        User user = await db.ResolveUserAsync(id);

        // Don't let mods accidentally spam the audit log
        bool alreadyAuthorized = await moderationService.ShowSensitiveDataAsync(CurrentUser!, user);
        if (alreadyAuthorized)
            return NoContent();

        AuditLogEntry entry = await moderationService.QuerySensitiveDataAsync(
            CurrentUser!,
            user,
            req.Reason
        );

        return Ok(moderationRenderer.RenderAuditLogEntry(entry));
    }
}
