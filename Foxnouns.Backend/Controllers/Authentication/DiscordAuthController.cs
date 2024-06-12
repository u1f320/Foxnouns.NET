using Foxnouns.Backend.Database;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/v2/auth/discord")]
public class DiscordAuthController(Config config, DatabaseContext db) : ApiControllerBase
{
    private void CheckRequirements()
    {
        if (config.DiscordAuth.ClientId == null || config.DiscordAuth.ClientSecret == null)
        {
            throw new ApiError.BadRequest("Discord authentication is not enabled on this instance.");
        }
    }
}