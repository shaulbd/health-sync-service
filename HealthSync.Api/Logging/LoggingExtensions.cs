using System.Runtime.InteropServices;
using Serilog;
using Serilog.Extensions.Hosting;
using Serilog.Sinks.SystemConsole.Themes;

namespace HealthSync.Api.Logging
{
	public static class LoggingExtensions
	{
		public static void AddSerilogLogger(this WebApplicationBuilder builder, string? path)
		{
			// Ensure the directory exists
			var logDir = Path.GetDirectoryName(path);
			if (!Directory.Exists(logDir))
			{
				Directory.CreateDirectory(logDir);
			}

			// Configure Serilog
			var logger = Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.With<SyncContextEnricher>()
				.WriteTo.Console(theme: AnsiConsoleTheme.Code)
				.WriteTo.File(
					path: path,
					rollingInterval: RollingInterval.Day,
					rollOnFileSizeLimit: true,
					buffered: true,
					flushToDiskInterval: TimeSpan.FromMinutes(1),
					outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] [{SyncContextId}] {Message:lj}{NewLine}{Exception}"
				)
				.CreateLogger();

			builder.Logging.ClearProviders();
			builder.Logging.AddSerilog(logger);
			builder.Services.AddSingleton(Log.Logger);
			builder.Services.AddSingleton<DiagnosticContext>();
		}
	}
}
