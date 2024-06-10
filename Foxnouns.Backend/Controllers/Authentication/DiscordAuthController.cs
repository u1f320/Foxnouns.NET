using Foxnouns.Backend.Database;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/v2/auth/discord")]
public class DiscordAuthController(Config config, DatabaseContext db) : ApiControllerBase
{
    [HttpPost("url")]
    public async Task<IActionResult> AuthenticationUrl()
    {
        throw new NotImplementedException();
    }
}