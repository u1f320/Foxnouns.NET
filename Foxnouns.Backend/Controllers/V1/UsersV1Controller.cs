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
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Services.V1;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers.V1;

[Route("/api/v1")]
public class UsersV1Controller(UsersV1Service usersV1Service, MembersV1Service membersV1Service)
    : ApiControllerBase
{
    [HttpGet("users/{userRef}")]
    public async Task<IActionResult> GetUserAsync(string userRef, CancellationToken ct = default)
    {
        User user = await usersV1Service.ResolveUserAsync(userRef, CurrentToken, ct);
        return Ok(
            await usersV1Service.RenderUserAsync(
                user,
                CurrentToken,
                renderMembers: true,
                renderFlags: true,
                ct: ct
            )
        );
    }

    [HttpGet("members/{id}")]
    public async Task<IActionResult> GetMemberAsync(string id, CancellationToken ct = default)
    {
        Member member = await membersV1Service.ResolveMemberAsync(id, ct);
        return Ok(
            await membersV1Service.RenderMemberAsync(
                member,
                CurrentToken,
                renderFlags: true,
                ct: ct
            )
        );
    }
}
