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
using System.ComponentModel.DataAnnotations.Schema;
using Foxnouns.Backend.Utils;
using Newtonsoft.Json;

namespace Foxnouns.Backend.Database.Models;

public class AuditLogEntry : BaseModel
{
    public Snowflake ModeratorId { get; init; }
    public string ModeratorUsername { get; init; } = string.Empty;
    public Snowflake? TargetUserId { get; init; }
    public string? TargetUsername { get; init; }
    public Snowflake? TargetMemberId { get; init; }
    public string? TargetMemberName { get; init; }
    public Snowflake? ReportId { get; init; }
    public Report? Report { get; init; }

    public AuditLogEntryType Type { get; init; }
    public string? Reason { get; init; }
    public string[]? ClearedFields { get; init; }
}

[JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))]
public enum AuditLogEntryType
{
    IgnoreReport,
    WarnUser,
    WarnUserAndClearProfile,
    SuspendUser,
}
