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
using Foxnouns.Backend.Utils;
using Newtonsoft.Json;

namespace Foxnouns.Backend.Database.Models;

public class Report : BaseModel
{
    public Snowflake ReporterId { get; init; }
    public User Reporter { get; init; } = null!;
    public Snowflake TargetUserId { get; init; }
    public User TargetUser { get; init; } = null!;

    public Snowflake? TargetMemberId { get; init; }
    public Member? TargetMember { get; init; }

    public ReportStatus Status { get; set; }
    public ReportReason Reason { get; init; }
    public string? Context { get; init; }

    public ReportTargetType TargetType { get; init; }
    public string? TargetSnapshot { get; init; }

    public AuditLogEntry? AuditLogEntry { get; set; }
}

[JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))]
public enum ReportTargetType
{
    User,
    Member,
}

[JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))]
public enum ReportStatus
{
    Open,
    Closed,
}

[JsonConverter(typeof(ScreamingSnakeCaseEnumConverter))]
public enum ReportReason
{
    Totalitarianism,
    HateSpeech,
    Racism,
    Homophobia,
    Transphobia,
    Queerphobia,
    Exclusionism,
    Sexism,
    Ableism,
    ChildPornography,
    PedophiliaAdvocacy,
    Harassment,
    Impersonation,
    Doxxing,
    EncouragingSelfHarm,
    Spam,
    Trolling,
    Advertisement,
    CopyrightViolation,
}
