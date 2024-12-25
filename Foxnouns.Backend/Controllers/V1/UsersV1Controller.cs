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
