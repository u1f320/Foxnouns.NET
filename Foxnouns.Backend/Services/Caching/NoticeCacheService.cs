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
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Services.Caching;

public class NoticeCacheService(IServiceProvider serviceProvider, IClock clock, ILogger logger)
    : SingletonCacheService<Notice>(serviceProvider, clock, logger)
{
    public override Duration MaxAge { get; init; } = Duration.FromMinutes(5);

    public override Func<
        DatabaseContext,
        CancellationToken,
        Task<Notice?>
    > FetchFunc { get; init; } =
        async (db, ct) =>
            await db
                .Notices.Where(n =>
                    n.StartTime < clock.GetCurrentInstant() && n.EndTime > clock.GetCurrentInstant()
                )
                .OrderByDescending(n => n.Id)
                .FirstOrDefaultAsync(ct);
}
