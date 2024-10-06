using Foxnouns.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers.Authentication;

[Route("/api/internal/auth/fediverse")]
public class FediverseAuthController(FediverseAuthService fediverseAuthService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<FediverseUrlResponse>(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFediverseUrlAsync([FromQuery] string instance)
    {
        var url = await fediverseAuthService.GenerateAuthUrlAsync(instance);
        return Ok(new FediverseUrlResponse(url));
    }

    public async Task<IActionResult> FediverseCallbackAsync([FromBody] CallbackRequest req)
    {
        throw new NotImplementedException();
    }

    public record CallbackRequest(string Instance, string Code);

    private record FediverseUrlResponse(string Url);
}
