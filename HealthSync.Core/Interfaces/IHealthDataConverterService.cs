using HealthSync.Core.Models;

namespace HealthSync.Core.Interfaces;

public interface IHealthDataConverterService
{
	Task CollectAndConvertDataAsync<T>(Func<Task<T>> dataFetcher, HealthData healthData, CancellationToken token);
}

public interface IHealthDataConverter { }