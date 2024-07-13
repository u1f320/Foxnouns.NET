using Foxnouns.Backend.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/meta")]
public class MetaController(DatabaseContext db, IClock clock) : ApiControllerBase
{
    private const string Repository = "https://codeberg.org/pronounscc/pronouns.cc";

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MetaResponse))]
    public async Task<IActionResult> GetMeta()
    {
        var now = clock.GetCurrentInstant();
        var users = await db.Users.Select(u => u.LastActive).ToListAsync();
        var memberCount = await db.Members.CountAsync();

        return Ok(new MetaResponse(
            Repository, BuildInfo.Version, BuildInfo.Hash, memberCount,
            new UserInfo(
                users.Count,
                users.Count(i => i > now - Duration.FromDays(30)),
                users.Count(i => i > now - Duration.FromDays(7)),
                users.Count(i => i > now - Duration.FromDays(1))
            ))
        );
    }

    [HttpGet("/api/v2/coffee")]
    public IActionResult BrewCoffee() => Problem("Sorry, I'm a teapot!", statusCode: StatusCodes.Status418ImATeapot);

    private record MetaResponse(string Repository, string Version, string Hash, int Members, UserInfo Users);

    private record UserInfo(int Total, int ActiveMonth, int ActiveWeek, int ActiveDay);
}