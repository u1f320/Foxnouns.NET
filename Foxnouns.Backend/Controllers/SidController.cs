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
using System.Diagnostics.CodeAnalysis;
using Foxnouns.Backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers;

[Route("/sid")]
[SuppressMessage(
    "Performance",
    "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons",
    Justification = "Not usable with EFCore"
)]
[ApiExplorerSettings(IgnoreApi = true)]
public class SidController(Config config, DatabaseContext db) : ApiControllerBase
{
    [HttpGet("{**id}")]
    public async Task<IActionResult> ResolveSidAsync(string id, CancellationToken ct = default) =>
        id.Length switch
        {
            5 => await ResolveUserSidAsync(id, ct),
            6 => await ResolveMemberSidAsync(id, ct),
            _ => Redirect(config.BaseUrl),
        };

    private async Task<IActionResult> ResolveUserSidAsync(string id, CancellationToken ct = default)
    {
        string? username = await db
            .Users.Where(u => u.Sid == id.ToLowerInvariant() && !u.Deleted)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(ct);
        if (username == null)
            return Redirect(config.BaseUrl);

        return Redirect($"{config.BaseUrl}/@{username}");
    }

    private async Task<IActionResult> ResolveMemberSidAsync(
        string id,
        CancellationToken ct = default
    )
    {
        var member = await db
            .Members.Include(m => m.User)
            .Where(m => m.Sid == id.ToLowerInvariant() && !m.User.Deleted)
            .Select(m => new { m.Name, m.User.Username })
            .FirstOrDefaultAsync(ct);
        if (member == null)
            return Redirect(config.BaseUrl);

        return Redirect($"{config.BaseUrl}/@{member.Username}/{member.Name}");
    }
}
