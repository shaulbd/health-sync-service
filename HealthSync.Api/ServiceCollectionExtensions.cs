using HealthSync.Core.Interfaces;
using HealthSync.Core.Shared;

namespace HealthSync.Api
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddHealthDataConverters(this IServiceCollection services)
		{
			// Register all converters
			var converterTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(IHealthDataConverter).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

			foreach (var type in converterTypes)
			{
				services.AddTransient(typeof(IHealthDataConverter), type);
			}

			// Register the converter service
			services.AddSingleton<IHealthDataConverterService, HealthDataConverterService>();

			return services;
		}
	}
}