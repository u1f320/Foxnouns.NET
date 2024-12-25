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
using Foxnouns.Backend.Dto.V1;
using Foxnouns.Backend.Services.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foxnouns.Backend.Controllers.V1;

[Route("/api/v1")]
public class V1ReadController(
    UsersV1Service usersV1Service,
    MembersV1Service membersV1Service,
    DatabaseContext db
) : ApiControllerBase
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

    [HttpGet("users/{userRef}/members")]
    public async Task<IActionResult> GetUserMembersAsync(
        string userRef,
        CancellationToken ct = default
    )
    {
        User user = await usersV1Service.ResolveUserAsync(userRef, CurrentToken, ct);
        List<Member> members = await db
            .Members.Where(m => m.UserId == user.Id)
            .OrderBy(m => m.Name)
            .ToListAsync(ct);

        List<MemberResponse> responses = [];
        foreach (Member member in members)
        {
            responses.Add(
                await membersV1Service.RenderMemberAsync(
                    member,
                    CurrentToken,
                    user,
                    renderFlags: true,
                    ct: ct
                )
            );
        }

        return Ok(responses);
    }

    [HttpGet("users/{userRef}/members/{memberRef}")]
    public async Task<IActionResult> GetUserMemberAsync(
        string userRef,
        string memberRef,
        CancellationToken ct = default
    )
    {
        Member member = await membersV1Service.ResolveMemberAsync(
            userRef,
            memberRef,
            CurrentToken,
            ct
        );
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
