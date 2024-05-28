using System.Diagnostics;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/users")]
public class UsersController(DatabaseContext db) : ApiControllerBase
{
    [HttpGet("{userRef}")]
    public async Task<IActionResult> GetUser(string userRef)
    {
        var user = await db.ResolveUserAsync(userRef);
        return Ok(user);
    }

    [HttpGet("@me")]
    [Authorize("identify")]
    public Task<IActionResult> GetMe()
    {
        throw new NotImplementedException();
    }

    [HttpPatch("@me")]
    public Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest req)
    {
        throw new NotImplementedException();
    }

    public record UpdateUserRequest(string? Username, string? DisplayName);
}