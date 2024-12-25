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

public class Member : BaseModel
{
    public required string Name { get; set; }
    public string Sid { get; set; } = string.Empty;
    public required string LegacyId { get; init; }
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? Avatar { get; set; }
    public string[] Links { get; set; } = [];
    public bool Unlisted { get; set; }

    public List<FieldEntry> Names { get; set; } = [];
    public List<Pronoun> Pronouns { get; set; } = [];
    public List<Field> Fields { get; set; } = [];

    public List<MemberFlag> ProfileFlags { get; set; } = [];

    public Snowflake UserId { get; init; }
    public User User { get; init; } = null!;
}
