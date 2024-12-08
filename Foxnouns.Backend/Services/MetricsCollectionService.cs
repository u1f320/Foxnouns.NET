using System.Diagnostics;
using Foxnouns.Backend.Database;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Prometheus;
using ITimer = Prometheus.ITimer;

namespace Foxnouns.Backend.Services;

public class MetricsCollectionService(ILogger logger, IServiceProvider services, IClock clock)
{
    private readonly ILogger _logger = logger.ForContext<MetricsCollectionService>();

    private static readonly Duration Month = Duration.FromDays(30);
    private static readonly Duration Week = Duration.FromDays(7);
    private static readonly Duration Day = Duration.FromDays(1);

    public async Task CollectMetricsAsync(CancellationToken ct = default)
    {
        ITimer timer = FoxnounsMetrics.MetricsCollectionTime.NewTimer();
        Instant now = clock.GetCurrentInstant();

        await using AsyncServiceScope scope = services.CreateAsyncScope();
        // ReSharper disable once SuggestVarOrType_SimpleTypes
        await using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        List<Instant>? users = await db
            .Users.Where(u => !u.Deleted)
            .Select(u => u.LastActive)
            .ToListAsync(ct);
        FoxnounsMetrics.UsersCount.Set(users.Count);
        FoxnounsMetrics.UsersActiveMonthCount.Set(users.Count(i => i > now - Month));
        FoxnounsMetrics.UsersActiveWeekCount.Set(users.Count(i => i > now - Week));
        FoxnounsMetrics.UsersActiveDayCount.Set(users.Count(i => i > now - Day));

        int memberCount = await db
            .Members.Include(m => m.User)
            .Where(m => !m.Unlisted && !m.User.ListHidden && !m.User.Deleted)
            .CountAsync(ct);
        FoxnounsMetrics.MemberCount.Set(memberCount);

        var process = Process.GetCurrentProcess();
        FoxnounsMetrics.ProcessPhysicalMemory.Set(process.WorkingSet64);
        FoxnounsMetrics.ProcessVirtualMemory.Set(process.VirtualMemorySize64);
        FoxnounsMetrics.ProcessPrivateMemory.Set(process.PrivateMemorySize64);
        FoxnounsMetrics.ProcessThreads.Set(process.Threads.Count);
        FoxnounsMetrics.ProcessHandles.Set(process.HandleCount);

        _logger.Information(
            "Collected metrics in {DurationMilliseconds} ms",
            timer.ObserveDuration().TotalMilliseconds
        );
    }
}

public class BackgroundMetricsCollectionService(
    ILogger logger,
    MetricsCollectionService metricsCollectionService
) : BackgroundService
{
    private readonly ILogger _logger = logger.ForContext<BackgroundMetricsCollectionService>();

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.Information("Metrics are disabled, periodically collecting metrics manually");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(ct))
        {
            _logger.Debug("Collecting metrics manually");
            await metricsCollectionService.CollectMetricsAsync(ct);
        }
    }
}
