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
using NodaTime;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/notifications")]
public class NotificationsController(
    DatabaseContext db,
    ModerationRendererService moderationRenderer,
    IClock clock
) : ApiControllerBase
{
    [HttpGet]
    [Authorize("user.moderation")]
    [Limit(UsableByDeletedUsers = true)]
    public async Task<IActionResult> GetNotificationsAsync([FromQuery] bool all = false)
    {
        IQueryable<Notification> query = db.Notifications.Where(n => n.TargetId == CurrentUser!.Id);
        if (!all)
            query = query.Where(n => n.AcknowledgedAt == null);

        List<Notification> notifications = await query.OrderByDescending(n => n.Id).ToListAsync();

        return Ok(notifications.Select(moderationRenderer.RenderNotification));
    }

    [HttpPut("{id}/ack")]
    [Authorize("user.moderation")]
    [Limit(UsableByDeletedUsers = true)]
    public async Task<IActionResult> AcknowledgeNotificationAsync(Snowflake id)
    {
        Notification? notification = await db.Notifications.FirstOrDefaultAsync(n =>
            n.TargetId == CurrentUser!.Id && n.Id == id
        );
        if (notification == null)
            throw new ApiError.NotFound("Notification not found.");

        if (notification.AcknowledgedAt != null)
            return Ok(moderationRenderer.RenderNotification(notification));

        notification.AcknowledgedAt = clock.GetCurrentInstant();
        db.Update(notification);
        await db.SaveChangesAsync();

        return Ok(moderationRenderer.RenderNotification(notification));
    }
}
