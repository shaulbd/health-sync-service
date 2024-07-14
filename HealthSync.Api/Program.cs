using HealthSync.Api;
using HealthSync.Api.Auth;
using HealthSync.Api.Middlewares;
using HealthSync.BackgroundServices.Services;
using HealthSync.Core;
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
const string SyncConfig = "sync-config.yml";

// Load settings from configuration
var settings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>();
if (settings == null)
{
	throw new ArgumentNullException(nameof(settings));
}

builder.Services.AddSingleton(settings);

// Add Serilog logger
builder.Services.AddSerilogLogger(builder.Configuration);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Add CORS policy
builder.Services.AddCors(options =>
{
	var allowed = settings.CorsSettings.AllowedOrigins.ToArray();
	allowed = allowed.Length != 0 ? allowed : ["*"];
	options.AddPolicy(CorsPolicy,
		builder =>
		{
			builder.WithOrigins(allowed)
				   .AllowAnyHeader()
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
	c.AddSwaggerAuth(settings.ApiKey); // Enable api key if set
});

// Load configuration for services
builder.Services.AddSingleton(CoreExtensions.LoadConfiguration(SyncConfig));

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

// Add error handling
builder.Services.AddErrorHandler();

var app = builder.Build();

// Configure global exception handler
app.UseExceptionHandler(_ => { });

// Configure Serilog request logging
app.UseSerilogRequestLogging(options =>
{
	options.MessageTemplate = "Handled {RequestPath}";
	options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;
	options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
	{
		diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
		diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
	};
});

// Configure the HTTP request pipeline
if (settings.EnableSwagger)
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
