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
using System.Diagnostics;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Extensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Extensions;

namespace Foxnouns.Backend.Services;

public class DataCleanupService(
    DatabaseContext db,
    IClock clock,
    ILogger logger,
    ObjectStorageService objectStorageService
)
{
    private readonly ILogger _logger = logger.ForContext<DataCleanupService>();

    public async Task InvokeAsync(CancellationToken ct = default)
    {
        _logger.Debug("Cleaning up expired users");
        await CleanUsersAsync(ct);

        _logger.Debug("Cleaning up expired data exports");
        await CleanExportsAsync(ct);
    }

    private async Task CleanUsersAsync(CancellationToken ct = default)
    {
        Instant selfDeleteExpires = clock.GetCurrentInstant() - User.DeleteAfter;
        Instant suspendExpires = clock.GetCurrentInstant() - User.DeleteSuspendedAfter;
        List<User> users = await db
            .Users.Include(u => u.Members)
            .Include(u => u.DataExports)
            .Where(u =>
                u.Deleted
                && (
                    (u.DeletedBy != null && u.DeletedAt < suspendExpires)
                    || (u.DeletedBy == null && u.DeletedAt < selfDeleteExpires)
                )
            )
            .OrderBy(u => u.Id)
            .AsSplitQuery()
            .ToListAsync(ct);
        if (users.Count == 0)
            return;

        _logger.Debug(
            "Deleting {Count} users that have been deleted for over 30 days or suspended for over 180 days",
            users.Count
        );

        var sw = new Stopwatch();

        await Task.WhenAll(users.Select(u => CleanUserAsync(u, ct)));

        await db.SaveChangesAsync(ct);
        _logger.Information(
            "Deleted {Count} users, their members, and their exports in {Time}",
            users.Count,
            sw.ElapsedDuration()
        );
    }

    private Task CleanUserAsync(User user, CancellationToken ct = default)
    {
        var tasks = new List<Task>();

        if (user.Avatar != null)
            tasks.Add(objectStorageService.DeleteUserAvatarAsync(user.Id, user.Avatar, ct));

        tasks.AddRange(
            user.Members.Select(member =>
                objectStorageService.DeleteMemberAvatarAsync(member.Id, member.Avatar!, ct)
            )
        );

        tasks.AddRange(
            user.DataExports.Select(export =>
                objectStorageService.RemoveObjectAsync(
                    ExportPath(export.UserId, export.Filename),
                    ct
                )
            )
        );

        db.Remove(user);
        return Task.WhenAll(tasks);
    }

    private async Task CleanExportsAsync(CancellationToken ct = default)
    {
        var minExpiredId = Snowflake.FromInstant(clock.GetCurrentInstant() - DataExport.Expiration);
        List<DataExport> exports = await db
            .DataExports.Where(d => d.Id < minExpiredId)
            .ToListAsync(ct);
        if (exports.Count == 0)
            return;

        _logger.Debug("Deleting {Count} expired exports", exports.Count);

        foreach (DataExport? export in exports)
        {
            _logger.Debug("Deleting export {ExportId}", export.Id);
            await objectStorageService.RemoveObjectAsync(
                ExportPath(export.UserId, export.Filename),
                ct
            );
            db.Remove(export);
        }

        await db.SaveChangesAsync(ct);
    }

    private static string ExportPath(Snowflake userId, string b64) =>
        $"data-exports/{userId}/{b64}.zip";
}
