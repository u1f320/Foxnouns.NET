using Foxnouns.Backend.Database;
using Foxnouns.Backend.Middleware;
using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users")]
public class UsersController(DatabaseContext db, UserRendererService userRendererService) : ApiControllerBase
{
    [HttpGet("{userRef}")]
    public async Task<IActionResult> GetUser(string userRef)
    {
        var user = await db.ResolveUserAsync(userRef);
        return Ok(await userRendererService.RenderUserAsync(user, selfUser: CurrentUser));
    }

    [HttpGet("@me")]
    [Authorize("identify")]
    public async Task<IActionResult> GetMe()
    {
        var user = await db.ResolveUserAsync(CurrentUser!.Id);
        return Ok(await userRendererService.RenderUserAsync(user, selfUser: CurrentUser));
    }

    [HttpPatch("@me")]
    public Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest req)
    {
        throw new NotImplementedException();
    }

    public record UpdateUserRequest(string? Username, string? DisplayName);
}