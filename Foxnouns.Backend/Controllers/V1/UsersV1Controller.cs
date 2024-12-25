using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Services.V1;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers.V1;

[Route("/api/v1/users")]
public class UsersV1Controller(UsersV1Service usersV1Service) : ApiControllerBase
{
    [HttpGet("{userRef}")]
    public async Task<IActionResult> GetUserAsync(string userRef, CancellationToken ct = default)
    {
        User user = await usersV1Service.ResolveUserAsync(userRef, CurrentToken, ct);
        return Ok(await usersV1Service.RenderUserAsync(user));
    }
}
