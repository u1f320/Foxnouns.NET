namespace Foxnouns.Backend.Services;

public class PeriodicTasksService(ILogger logger, IServiceProvider services) : BackgroundService
{
    private readonly ILogger _logger = logger.ForContext<PeriodicTasksService>();

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(ct))
        {
            _logger.Debug("Collecting metrics");
            await RunPeriodicTasksAsync(ct);
        }
    }

    private async Task RunPeriodicTasksAsync(CancellationToken ct)
    {
        _logger.Debug("Running periodic tasks");

        await using var scope = services.CreateAsyncScope();

        var keyCacheSvc = scope.ServiceProvider.GetRequiredService<KeyCacheService>();
        await keyCacheSvc.DeleteExpiredKeysAsync(ct);
    }
}
