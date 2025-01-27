using HealthSync.Api;
using HealthSync.Api.Auth;
using HealthSync.Api.Logging;
using HealthSync.Api.Middlewares;
using HealthSync.BackgroundServices.Services;
using HealthSync.Core.Extensions;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Services;
using HealthSync.Plugin.GarminConnect;
using HealthSync.Plugin.InfluxDb;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "CorsPolicy";

// Add Serilog logger
var logPath = (builder.Configuration["LOG_PATH"] ?? "logs/applog-.txt").NormalizePath(PathNormalizer.UserData);
builder.AddSerilogLogger(logPath);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Add CORS policy
builder.Services.AddCors(options =>
{
	options.AddPolicy(CorsPolicy, policyBuilder =>
	{
		var corsHosts = (builder.Configuration["CORS_HOSTS"] ?? "*")
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (corsHosts.Contains("*"))
		{
			policyBuilder.AllowAnyOrigin();
		}
		else
		{
			policyBuilder.WithOrigins(corsHosts);
		}
		policyBuilder.AllowAnyHeader()
					 .AllowAnyMethod();
	});
});

// Add OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Health Sync API", Version = "v1" });
	c.ExampleFilters(); // Enable examples
	c.AddSwaggerAuth(builder.Configuration["API_KEY"]); // Enable api key if set
});

// Load configuration for services
var syncConfigPath = (builder.Configuration["SYNC_CONFIG_PATH"] ?? "sync-config.yml").NormalizePath(PathNormalizer.UserConfig);
builder.Services.AddSingleton(CoreExtensions.LoadConfiguration(syncConfigPath));

// Set up plugins
builder.Services.AddKeyedSingleton<IEnumerable<Assembly>>("Plugins",
	[
		typeof(Program).Assembly,
		typeof(GarminConnectPlugin).Assembly,
		typeof(InfluxDbPlugin).Assembly
	]);

// Register PluginFactory
builder.Services.AddSingleton<PluginFactory>();

// Register SyncBackgroundService as IHostedService and singleton
builder.Services.AddHostedService<SyncBackgroundService>();
builder.Services.AddSingleton<SyncBackgroundService>();

// Register SemaphoreManager for sync locking
builder.Services.AddSingleton<SemaphoreManager>();

// Register health converters
builder.Services.AddHealthDataConverters();

// Register cache
var dbPath = (builder.Configuration["SYNC_DB_FILE"] ?? "sqlite.cache.db").NormalizePath(PathNormalizer.UserCache);
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IPersistentStorage, SqlitePersistentStorage>(sp =>
	new SqlitePersistentStorage(dbPath, sp.GetRequiredService<ILogger<SqlitePersistentStorage>>()));
builder.Services.AddSingleton<IHybridCache, HybridCache>();
builder.Services.AddHostedService(sp =>
            new CachePersistenceBackgroundService(
                sp.GetRequiredService<IHybridCache>(),
                TimeSpan.FromMinutes(1)));
builder.Services.AddTransient(typeof(IPluginCache<>), typeof(PluginCache<>));

// Add error handling
builder.Services.AddErrorHandler();

var app = builder.Build();

// Configure global exception handler
app.UseExceptionHandler(_ => { });

// Configure Serilog request logging
app.UseSerilogRequestLogging(options =>
{
	options.MessageTemplate = "Handled {RequestPath}";
	options.GetLevel = (httpContext, elapsed, ex) => builder.Environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Error;
	options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
	{
		diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
		diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
	};
});

// Configure the HTTP request pipeline
if (bool.TryParse(app.Configuration["API_EXPLORER"], out bool enableSwagger) && enableSwagger)
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.MapSwagger().RequireAuthorization();
app.UseCors(CorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();
