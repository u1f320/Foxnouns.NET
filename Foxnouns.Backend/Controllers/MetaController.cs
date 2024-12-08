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
                new UserInfo(
                    (int)FoxnounsMetrics.UsersCount.Value,
                    (int)FoxnounsMetrics.UsersActiveMonthCount.Value,
                    (int)FoxnounsMetrics.UsersActiveWeekCount.Value,
                    (int)FoxnounsMetrics.UsersActiveDayCount.Value
                ),
                new Limits(
                    MembersController.MaxMemberCount,
                    ValidationUtils.MaxBioLength,
                    ValidationUtils.MaxCustomPreferences,
                    AuthUtils.MaxAuthMethodsPerType
                )
            )
        );

    [HttpGet("/api/v2/coffee")]
    public IActionResult BrewCoffee() =>
        Problem("Sorry, I'm a teapot!", statusCode: StatusCodes.Status418ImATeapot);

    private record MetaResponse(
        string Repository,
        string Version,
        string Hash,
        int Members,
        UserInfo Users,
        Limits Limits
    );

    private record UserInfo(int Total, int ActiveMonth, int ActiveWeek, int ActiveDay);

    // All limits that the frontend should know about (for UI purposes)
    private record Limits(
        int MemberCount,
        int BioLength,
        int CustomPreferences,
        int MaxAuthMethods
    );
}
