using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

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
        _logger.Information("Cleaning up expired data exports");
        await CleanExportsAsync(ct);
    }

    private async Task CleanExportsAsync(CancellationToken ct = default)
    {
        var minExpiredId = Snowflake.FromInstant(clock.GetCurrentInstant() - DataExport.Expiration);
        var exports = await db.DataExports.Where(d => d.Id < minExpiredId).ToListAsync(ct);
        if (exports.Count == 0)
            return;

        _logger.Debug("There are {Count} expired exports", exports.Count);

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
