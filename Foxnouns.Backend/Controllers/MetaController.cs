using Foxnouns.Backend.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/meta")]
public class MetaController(DatabaseContext db) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MetaResponse))]
    public async Task<IActionResult> GetMeta()
    {
        var userCount = await db.Users.CountAsync();
        var memberCount = await db.Members.CountAsync();

        return Ok(new MetaResponse(
            BuildInfo.Version, BuildInfo.Hash, memberCount,
            new UserInfo(userCount, 0, 0, 0))
        );
    }

    [HttpGet("coffee")]
    public IActionResult BrewCoffee() => Problem("Sorry, I'm a teapot!", statusCode: StatusCodes.Status418ImATeapot);

    private record MetaResponse(string Version, string Hash, int Members, UserInfo Users);

    private record UserInfo(int Total, int ActiveMonth, int ActiveWeek, int ActiveDay);
}