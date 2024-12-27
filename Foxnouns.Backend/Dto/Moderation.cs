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

// ReSharper disable NotAccessedPositionalProperty.Global
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;

namespace Foxnouns.Backend.Dto;

public record ReportResponse(
    Snowflake Id,
    PartialUser Reporter,
    PartialUser TargetUser,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        PartialMember? TargetMember,
    ReportStatus Status,
    ReportReason Reason,
    string? Context,
    ReportTargetType TargetType,
    JObject? Snapshot
);

public record AuditLogResponse(
    Snowflake Id,
    AuditLogEntity Moderator,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        AuditLogEntity? TargetUser,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        AuditLogEntity? TargetMember,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] PartialReport? Report,
    AuditLogEntryType Type,
    string? Reason,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string[]? ClearedFields
);

public record PartialReport(
    Snowflake Id,
    Snowflake ReporterId,
    Snowflake TargetUserId,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        Snowflake? TargetMemberId,
    ReportReason Reason,
    string? Context,
    ReportTargetType TargetType
);

public record NotificationResponse(
    Snowflake Id,
    NotificationType Type,
    string? Message,
    string? LocalizationKey,
    Dictionary<string, string> LocalizationParams,
    bool Acknowledged
);

public record AuditLogEntity(Snowflake Id, string Username);

public record CreateReportRequest(ReportReason Reason, string? Context = null);

public record IgnoreReportRequest(string? Reason = null);

public record WarnUserRequest(
    string Reason,
    FieldsToClear[]? ClearFields = null,
    Snowflake? MemberId = null,
    Snowflake? ReportId = null
);

public record SuspendUserRequest(string Reason, bool ClearProfile, Snowflake? ReportId = null);

public enum FieldsToClear
{
    DisplayName,
    Avatar,
    Bio,
    Links,
    Names,
    Pronouns,
    Fields,
    Flags,
    CustomPreferences,
}

public record QueryUsersRequest(string Query, bool Fuzzy);

public record QueryUserResponse(
    UserResponse User,
    bool MemberListHidden,
    Instant LastActive,
    Instant LastSidReroll,
    bool Suspended,
    bool Deleted,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        IEnumerable<AuthMethodResponse>? AuthMethods
);

public record QuerySensitiveUserDataRequest(string Reason);
