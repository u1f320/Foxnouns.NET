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

        return Ok(new MetaResponse(userCount, memberCount, BuildInfo.Version, BuildInfo.Hash));
    }

    private record MetaResponse(int Users, int Members, string Version, string Hash);
}