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
        _logger.Information("Cleaning up expired users");
        await CleanUsersAsync(ct);

        _logger.Information("Cleaning up expired data exports");
        await CleanExportsAsync(ct);
    }

    private async Task CleanUsersAsync(CancellationToken ct = default)
    {
        var selfDeleteExpires = clock.GetCurrentInstant() - User.DeleteAfter;
        var suspendExpires = clock.GetCurrentInstant() - User.DeleteSuspendedAfter;
        var users = await db
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
        var exports = await db.DataExports.Where(d => d.Id < minExpiredId).ToListAsync(ct);
        if (exports.Count == 0)
            return;

        _logger.Debug("Deleting {Count} expired exports", exports.Count);

        foreach (var export in exports)
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
