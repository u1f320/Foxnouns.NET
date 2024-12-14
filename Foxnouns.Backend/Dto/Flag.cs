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
// ReSharper disable UnusedAutoPropertyAccessor.Global
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Utils;

namespace Foxnouns.Backend.Dto;

public record PrideFlagResponse(Snowflake Id, string? ImageUrl, string Name, string? Description);

public record CreateFlagRequest(string Name, string Image, string? Description);

public class UpdateFlagRequest : PatchRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
}
