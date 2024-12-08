using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/meta")]
public class MetaController : ApiControllerBase
{
    private const string Repository = "https://codeberg.org/pronounscc/pronouns.cc";

    [HttpGet]
    [ProducesResponseType<MetaResponse>(StatusCodes.Status200OK)]
    public IActionResult GetMeta() =>
        Ok(
            new MetaResponse(
                Repository,
                BuildInfo.Version,
                BuildInfo.Hash,
                (int)FoxnounsMetrics.MemberCount.Value,
                new UserInfoResponse(
                    (int)FoxnounsMetrics.UsersCount.Value,
                    (int)FoxnounsMetrics.UsersActiveMonthCount.Value,
                    (int)FoxnounsMetrics.UsersActiveWeekCount.Value,
                    (int)FoxnounsMetrics.UsersActiveDayCount.Value
                ),
                new LimitsResponse(
                    MembersController.MaxMemberCount,
                    ValidationUtils.MaxBioLength,
                    ValidationUtils.MaxCustomPreferences,
                    AuthUtils.MaxAuthMethodsPerType,
                    FlagsController.MaxFlagCount
                )
            )
        );

    [HttpGet("/api/v2/coffee")]
    public IActionResult BrewCoffee() =>
        Problem("Sorry, I'm a teapot!", statusCode: StatusCodes.Status418ImATeapot);
}
