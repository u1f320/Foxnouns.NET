using Foxnouns.Backend.Database;
using Foxnouns.Backend.Jobs;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users")]
public class UsersController(DatabaseContext db, UserRendererService userRendererService) : ApiControllerBase
{
    [HttpGet("{userRef}")]
    public async Task<IActionResult> GetUserAsync(string userRef)
    {
        var user = await db.ResolveUserAsync(userRef);
        return Ok(await userRendererService.RenderUserAsync(user, selfUser: CurrentUser));
    }

    [HttpGet("@me")]
    [Authorize("identify")]
    public async Task<IActionResult> GetMeAsync()
    {
        var user = await db.ResolveUserAsync(CurrentUser!.Id);
        return Ok(await userRendererService.RenderUserAsync(user, selfUser: CurrentUser));
    }

    [HttpPatch("@me")]
    public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserRequest req)
    {
        if (req.Avatar != null)
            AvatarUpdateJob.QueueUpdateUserAvatar(CurrentUser!.Id, req.Avatar);

        return NoContent();
    }

    public record UpdateUserRequest(string? Username, string? DisplayName, string? Avatar);
}