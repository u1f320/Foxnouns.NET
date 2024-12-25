// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.Text.RegularExpressions;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/meta")]
public partial class MetaController : ApiControllerBase
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

    [HttpGet("page/{page}")]
    public async Task<IActionResult> GetStaticPageAsync(string page, CancellationToken ct = default)
    {
        if (!PageRegex().IsMatch(page))
        {
            throw new ApiError.BadRequest("Invalid page name");
        }

        string path = Path.Join(Directory.GetCurrentDirectory(), "static-pages", $"{page}.md");
        string text = await System.IO.File.ReadAllTextAsync(path, ct);
        return Ok(text);
    }

    [HttpGet("/api/v2/coffee")]
    public IActionResult BrewCoffee() =>
        Problem("Sorry, I'm a teapot!", statusCode: StatusCodes.Status418ImATeapot);

    [GeneratedRegex(@"^[a-z\-_]+$")]
    private static partial Regex PageRegex();
}
