using HealthSync.Api.Logging;
using Serilog;

namespace HealthSync.Api.Auth
{
	public static class LoggingExtensions
	{
		public static void AddSerilogLogger(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSerilog(config =>
			{
				config.ReadFrom.Configuration(configuration).Enrich.With<SyncContextEnricher>();
			});
		}
	}
}
