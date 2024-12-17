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
using Foxnouns.Backend.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Foxnouns.Backend.Services;

public class ModerationRendererService(
    DatabaseContext db,
    UserRendererService userRenderer,
    MemberRendererService memberRenderer
)
{
    public ReportResponse RenderReport(Report report)
    {
        return new ReportResponse(
            report.Id,
            userRenderer.RenderPartialUser(report.Reporter),
            userRenderer.RenderPartialUser(report.TargetUser),
            report.TargetMemberId != null
                ? memberRenderer.RenderPartialMember(report.TargetMember!)
                : null,
            report.Status,
            report.Reason,
            report.TargetType,
            report.TargetSnapshot != null
                ? JsonConvert.DeserializeObject<JObject>(report.TargetSnapshot)
                : null
        );
    }

    public AuditLogResponse RenderAuditLogEntry(AuditLogEntry entry)
    {
        return new AuditLogResponse(
            Id: entry.Id,
            Moderator: ToEntity(entry.ModeratorId, entry.ModeratorUsername)!,
            TargetUser: ToEntity(entry.TargetUserId, entry.TargetUsername),
            TargetMember: ToEntity(entry.TargetMemberId, entry.TargetMemberName),
            ReportId: entry.ReportId,
            Type: entry.Type,
            Reason: entry.Reason,
            ClearedFields: entry.ClearedFields
        );
    }

    public NotificationResponse RenderNotification(Notification notification) =>
        new(
            notification.Id,
            notification.Type,
            notification.Message,
            notification.LocalizationKey,
            notification.LocalizationParams,
            notification.AcknowledgedAt != null
        );

    private static AuditLogEntity? ToEntity(Snowflake? id, string? username) =>
        id != null && username != null ? new AuditLogEntity(id.Value, username) : null;
}
