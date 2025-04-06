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
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Dto;
using Foxnouns.Backend.Services.Caching;
using Foxnouns.Backend.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers;

[Route("/api/v2/meta")]
public partial class MetaController(Config config, NoticeCacheService noticeCache)
    : ApiControllerBase
{
    private const string Repository = "https://codeberg.org/pronounscc/pronouns.cc";

    [HttpGet]
    [ProducesResponseType<MetaResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMeta(CancellationToken ct = default) =>
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
                    config.Limits.MaxMemberCount,
                    config.Limits.MaxBioLength,
                    ValidationUtils.MaxCustomPreferences,
                    AuthUtils.MaxAuthMethodsPerType,
                    FlagsController.MaxFlagCount
                ),
                Notice: NoticeResponse(await noticeCache.GetAsync(ct))
            )
        );

    private static MetaNoticeResponse? NoticeResponse(Notice? notice) =>
        notice == null ? null : new MetaNoticeResponse(notice.Id, notice.Message);

    [HttpGet("page/{page}")]
    public async Task<IActionResult> GetStaticPageAsync(string page, CancellationToken ct = default)
    {
        if (!PageRegex().IsMatch(page))
        {
            throw new ApiError.BadRequest("Invalid page name");
        }

        string path = Path.Join(Directory.GetCurrentDirectory(), "static-pages", $"{page}.md");
        try
        {
            string text = await System.IO.File.ReadAllTextAsync(path, ct);
            return Ok(text);
        }
        catch (FileNotFoundException)
        {
            throw new ApiError.NotFound("Page not found", code: ErrorCode.PageNotFound);
        }
    }

    [HttpGet("/api/v2/coffee")]
    public IActionResult BrewCoffee() =>
        StatusCode(StatusCodes.Status418ImATeapot, "Sorry, I'm a teapot!");

    [GeneratedRegex(@"^[a-z\-_]+$")]
    private static partial Regex PageRegex();
}
