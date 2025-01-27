using HealthSync.Core.Interfaces;
using HealthSync.Core.Models;
using Microsoft.Extensions.Logging;

namespace HealthSync.Core.Shared
{
	public class HealthDataConverterService(ILogger<HealthDataConverterService> logger, IEnumerable<IHealthDataConverter> converters) : IHealthDataConverterService
	{
		private readonly ILogger<HealthDataConverterService> _logger = logger;
		private readonly Dictionary<Type, IHealthDataConverter> _converters = converters.ToDictionary(c => c.GetType().BaseType.GetGenericArguments()[0]);

		public async Task CollectAndConvertDataAsync<T>(Func<Task<T>> dataFetcher, HealthData healthData, CancellationToken token = default)
		{
			var data = await dataFetcher();
			ConvertAndAdd(healthData, data, token);
		}

		private void ConvertAndAdd<TSource>(HealthData healthData, TSource sourceData, CancellationToken token)
		{
			if (_converters.TryGetValue(typeof(TSource), out var converter))
			{
				((HealthDataConverter<TSource>)converter).ConvertAndAdd(healthData, sourceData);
			}
			else
			{
				_logger.LogWarning($"No converter found for type {typeof(TSource).Name}");
			}
		}
	}
}
