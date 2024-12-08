namespace Foxnouns.Backend.Services;

public class PeriodicTasksService(ILogger logger, IServiceProvider services) : BackgroundService
{
    private readonly ILogger _logger = logger.ForContext<PeriodicTasksService>();

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(ct))
            await RunPeriodicTasksAsync(ct);
    }

    private async Task RunPeriodicTasksAsync(CancellationToken ct)
    {
        _logger.Debug("Running periodic tasks");

        await using AsyncServiceScope scope = services.CreateAsyncScope();

        // The type is literally written on the same line, we can just use `var`
        // ReSharper disable SuggestVarOrType_SimpleTypes
        var keyCacheService = scope.ServiceProvider.GetRequiredService<KeyCacheService>();
        var dataCleanupService = scope.ServiceProvider.GetRequiredService<DataCleanupService>();
        // ReSharper restore SuggestVarOrType_SimpleTypes

        await keyCacheService.DeleteExpiredKeysAsync(ct);
        await dataCleanupService.InvokeAsync(ct);
    }
}
