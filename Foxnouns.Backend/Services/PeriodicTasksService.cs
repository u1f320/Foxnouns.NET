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
