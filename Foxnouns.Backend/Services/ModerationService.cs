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
using Humanizer;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Services;

public class ModerationService(
    ILogger logger,
    DatabaseContext db,
    ISnowflakeGenerator snowflakeGenerator,
    IQueue queue,
    IClock clock
)
{
    private readonly ILogger _logger = logger.ForContext<ModerationService>();

    public async Task<AuditLogEntry> IgnoreReportAsync(
        User moderator,
        Report report,
        string? reason = null
    )
    {
        _logger.Information(
            "Moderator {ModeratorId} is ignoring report {ReportId} on user {TargetId}",
            moderator.Id,
            report.Id,
            report.TargetUserId
        );

        var entry = new AuditLogEntry
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            ModeratorId = moderator.Id,
            ModeratorUsername = moderator.Username,
            ReportId = report.Id,
            Type = AuditLogEntryType.IgnoreReport,
            Reason = reason,
        };
        db.AuditLog.Add(entry);

        report.Status = ReportStatus.Closed;
        db.Update(report);

        await db.SaveChangesAsync();
        return entry;
    }

    public async Task<AuditLogEntry> QuerySensitiveDataAsync(
        User moderator,
        User target,
        string reason
    )
    {
        _logger.Information(
            "Moderator {ModeratorId} is querying sensitive data for {TargetId}",
            moderator.Id,
            target.Id
        );

        var entry = new AuditLogEntry
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            ModeratorId = moderator.Id,
            ModeratorUsername = moderator.Username,
            TargetUserId = target.Id,
            TargetUsername = target.Username,
            Type = AuditLogEntryType.QuerySensitiveUserData,
            Reason = reason,
        };
        db.AuditLog.Add(entry);

        await db.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> ShowSensitiveDataAsync(
        User moderator,
        User target,
        CancellationToken ct = default
    )
    {
        Snowflake cutoff = snowflakeGenerator.GenerateSnowflake(
            clock.GetCurrentInstant() - Duration.FromDays(1)
        );

        return await db.AuditLog.AnyAsync(
            e =>
                e.ModeratorId == moderator.Id
                && e.TargetUserId == target.Id
                && e.Type == AuditLogEntryType.QuerySensitiveUserData
                && e.Id > cutoff,
            ct
        );
    }

    public async Task<AuditLogEntry> ExecuteSuspensionAsync(
        User moderator,
        User target,
        Report? report,
        string reason,
        bool clearProfile
    )
    {
        _logger.Information(
            "Moderator {ModeratorId} is suspending user {TargetId}",
            moderator.Id,
            target.Id
        );
        var entry = new AuditLogEntry
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            ModeratorId = moderator.Id,
            ModeratorUsername = moderator.Username,
            TargetUserId = target.Id,
            TargetUsername = target.Username,
            ReportId = report?.Id,
            Type = AuditLogEntryType.SuspendUser,
            Reason = reason,
        };
        db.AuditLog.Add(entry);

        db.Notifications.Add(
            new Notification
            {
                Id = snowflakeGenerator.GenerateSnowflake(),
                TargetId = target.Id,
                Type = NotificationType.Warning,
                Message = null,
                LocalizationKey = "notification.suspension",
                LocalizationParams = { { "reason", reason } },
            }
        );

        target.Deleted = true;
        target.DeletedAt = clock.GetCurrentInstant();
        target.DeletedBy = moderator.Id;

        if (!clearProfile)
        {
            db.Update(target);
            await db.SaveChangesAsync();
            return entry;
        }

        _logger.Information("Clearing profile of user {TargetId}", target.Id);

        target.Username = $"deleted-user-{target.Id}";
        target.DisplayName = null;
        target.Bio = null;
        target.MemberTitle = null;
        target.Links = [];
        target.Timezone = null;
        target.Names = [];
        target.Pronouns = [];
        target.Fields = [];
        target.CustomPreferences = [];
        target.ProfileFlags = [];

        queue.QueueInvocableWithPayload<UserAvatarUpdateInvocable, AvatarUpdatePayload>(
            new AvatarUpdatePayload(target.Id, null)
        );

        // TODO: also clear member profiles?

        db.Update(target);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task<AuditLogEntry> ExecuteWarningAsync(
        User moderator,
        User targetUser,
        Member? targetMember,
        Report? report,
        string reason,
        FieldsToClear[]? fieldsToClear
    )
    {
        _logger.Information(
            "Moderator {ModeratorId} is warning user {TargetId} (member {TargetMemberId})",
            moderator.Id,
            targetUser.Id,
            targetMember?.Id
        );

        string[]? fields = fieldsToClear?.Select(f => f.Humanize(LetterCasing.LowerCase)).ToArray();

        var entry = new AuditLogEntry
        {
            Id = snowflakeGenerator.GenerateSnowflake(),
            ModeratorId = moderator.Id,
            ModeratorUsername = moderator.Username,
            TargetUserId = targetUser.Id,
            TargetUsername = targetUser.Username,
            TargetMemberId = targetMember?.Id,
            TargetMemberName = targetMember?.Name,
            ReportId = report?.Id,
            Type =
                fields != null
                    ? AuditLogEntryType.WarnUserAndClearProfile
                    : AuditLogEntryType.WarnUser,
            Reason = reason,
            ClearedFields = fields,
        };
        db.AuditLog.Add(entry);

        db.Notifications.Add(
            new Notification
            {
                Id = snowflakeGenerator.GenerateSnowflake(),
                TargetId = targetUser.Id,
                Type = NotificationType.Warning,
                Message = null,
                LocalizationKey =
                    fieldsToClear != null
                        ? "notification.warning-cleared-fields"
                        : "notification.warning",
                LocalizationParams =
                {
                    { "reason", reason },
                    {
                        "clearedFields",
                        string.Join(
                            "\n",
                            fieldsToClear?.Select(f => f.Humanize(LetterCasing.LowerCase)) ?? []
                        )
                    },
                },
            }
        );

        if (targetMember != null && fieldsToClear != null)
        {
            foreach (FieldsToClear field in fieldsToClear)
            {
                switch (field)
                {
                    case FieldsToClear.DisplayName:
                        targetMember.DisplayName = null;
                        break;
                    case FieldsToClear.Avatar:
                        queue.QueueInvocableWithPayload<
                            MemberAvatarUpdateInvocable,
                            AvatarUpdatePayload
                        >(new AvatarUpdatePayload(targetMember.Id, null));
                        break;
                    case FieldsToClear.Bio:
                        targetMember.Bio = null;
                        break;
                    case FieldsToClear.Links:
                        targetMember.Links = [];
                        break;
                    case FieldsToClear.Names:
                        targetMember.Names = [];
                        break;
                    case FieldsToClear.Pronouns:
                        targetMember.Pronouns = [];
                        break;
                    case FieldsToClear.Fields:
                        targetMember.Fields = [];
                        break;
                    case FieldsToClear.Flags:
                        targetMember.ProfileFlags = [];
                        break;
                    // custom preferences can't be cleared on member-scoped warnings
                    case FieldsToClear.CustomPreferences:
                    default:
                        break;
                }
            }

            db.Update(targetMember);
        }
        else if (fieldsToClear != null)
        {
            foreach (FieldsToClear field in fieldsToClear)
            {
                switch (field)
                {
                    case FieldsToClear.DisplayName:
                        targetUser.DisplayName = null;
                        break;
                    case FieldsToClear.Avatar:
                        queue.QueueInvocableWithPayload<
                            UserAvatarUpdateInvocable,
                            AvatarUpdatePayload
                        >(new AvatarUpdatePayload(targetUser.Id, null));
                        break;
                    case FieldsToClear.Bio:
                        targetUser.Bio = null;
                        break;
                    case FieldsToClear.Links:
                        targetUser.Links = [];
                        break;
                    case FieldsToClear.Names:
                        targetUser.Names = [];
                        break;
                    case FieldsToClear.Pronouns:
                        targetUser.Pronouns = [];
                        break;
                    case FieldsToClear.Fields:
                        targetUser.Fields = [];
                        break;
                    case FieldsToClear.Flags:
                        targetUser.ProfileFlags = [];
                        break;
                    case FieldsToClear.CustomPreferences:
                        targetUser.CustomPreferences = [];
                        break;
                    default:
                        break;
                }
            }

            db.Update(targetUser);
        }

        await db.SaveChangesAsync();

        return entry;
    }
}
