using System.Diagnostics;
using Foxnouns.Backend.Database;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Prometheus;

namespace Foxnouns.Backend.Services;

public class MetricsCollectionService(
    ILogger logger,
    IServiceProvider services,
    IClock clock)
{
    private readonly ILogger _logger = logger.ForContext<MetricsCollectionService>();

    private static readonly Duration Month = Duration.FromDays(30);
    private static readonly Duration Week = Duration.FromDays(7);
    private static readonly Duration Day = Duration.FromDays(1);

    public async Task CollectMetricsAsync(CancellationToken ct = default)
    {
        var timer = FoxnounsMetrics.MetricsCollectionTime.NewTimer();
        var now = clock.GetCurrentInstant();

        await using var scope = services.CreateAsyncScope();
        await using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var users = await db.Users.Where(u => !u.Deleted).Select(u => u.LastActive).ToListAsync(ct);
        FoxnounsMetrics.UsersCount.Set(users.Count);
        FoxnounsMetrics.UsersActiveMonthCount.Set(users.Count(i => i > now - Month));
        FoxnounsMetrics.UsersActiveWeekCount.Set(users.Count(i => i > now - Week));
        FoxnounsMetrics.UsersActiveDayCount.Set(users.Count(i => i > now - Day));

        var memberCount = await db.Members.Include(m => m.User)
            .Where(m => !m.Unlisted && !m.User.ListHidden && !m.User.Deleted).CountAsync(ct);
        FoxnounsMetrics.MemberCount.Set(memberCount);

        var process = Process.GetCurrentProcess();
        FoxnounsMetrics.ProcessPhysicalMemory.Set(process.WorkingSet64);
        FoxnounsMetrics.ProcessVirtualMemory.Set(process.VirtualMemorySize64);
        FoxnounsMetrics.ProcessPrivateMemory.Set(process.PrivateMemorySize64);
        FoxnounsMetrics.ProcessThreads.Set(process.Threads.Count);
        FoxnounsMetrics.ProcessHandles.Set(process.HandleCount);

        _logger.Information("Collected metrics in {DurationMilliseconds} ms",
            timer.ObserveDuration().TotalMilliseconds);
    }
}

public class BackgroundMetricsCollectionService(ILogger logger, MetricsCollectionService metricsCollectionService)
    : BackgroundService
{
    private readonly ILogger _logger = logger.ForContext<BackgroundMetricsCollectionService>();

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.Information("Metrics are disabled, periodically collecting metrics manually");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(ct))
        {
            _logger.Debug("Collecting metrics");
            await metricsCollectionService.CollectMetricsAsync(ct);
        }
    }
}