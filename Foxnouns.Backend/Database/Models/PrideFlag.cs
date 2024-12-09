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
namespace Foxnouns.Backend.Database.Models;

public class PrideFlag : BaseModel
{
    public required Snowflake UserId { get; init; }

    // A null hash means the flag hasn't been processed yet.
    public string? Hash { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class UserFlag
{
    public long Id { get; init; }
    public required Snowflake UserId { get; init; }
    public required Snowflake PrideFlagId { get; init; }
    public PrideFlag PrideFlag { get; init; } = null!;
}

public class MemberFlag
{
    public long Id { get; init; }
    public required Snowflake MemberId { get; init; }
    public required Snowflake PrideFlagId { get; init; }
    public PrideFlag PrideFlag { get; init; } = null!;
}
