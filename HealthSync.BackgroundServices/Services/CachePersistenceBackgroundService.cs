using HealthSync.Core.Interfaces;
using HealthSync.Core.Services;
using Microsoft.Extensions.Hosting;

namespace HealthSync.BackgroundServices.Services;

public class CachePersistenceBackgroundService(IHybridCache hybridCache, TimeSpan persistInterval) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(persistInterval, stoppingToken);
            await hybridCache.PersistPendingWritesAsync();
        }
    }
}