using System.Net;
using System.Text.Json.Nodes;
using HealthSync.Core.Extensions;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Models;
using HealthSync.Plugin.GarminConnect.Client;
using HealthSync.Plugin.GarminConnect.Client.Auth;
using HealthSync.Plugin.GarminConnect.Client.Auth.Models;
using HealthSync.Plugin.GarminConnect.Client.Models;
using Microsoft.Extensions.Logging;

namespace HealthSync.Plugin.GarminConnect
{
	/// <summary>
	/// Garmin Connect provider plugin
	/// </summary>
	/// <param name="logger"></param>
	/// <param name="syncContext"></param>
	public class GarminConnectPlugin(
		ILogger<GarminConnectPlugin> logger,
		SyncContext syncContext,
		IHealthDataConverterService converter,
		IPluginCache<GarminConnectPlugin> cache) : IProviderPlugin
	{
		/// <summary>
		/// Friendly Name
		/// </summary>
		public string Name => "GarminConnect";

		/// <summary>
		/// Unique ID. DO NOT CHANGE
		/// </summary>
		public static string UniqueId => "garmin-connect";

		public ILogger<GarminConnectPlugin> Logger { get; } = logger;
		public SyncContext SyncContext { get; } = syncContext;

		public async Task<HealthData> GetAsync(
			DateTimeOffset start,
			DateTimeOffset end,
			string index,
			CancellationToken token = default)
		{
			ArgumentNullException.ThrowIfNull(start);
			ArgumentNullException.ThrowIfNull(end);
			ArgumentException.ThrowIfNullOrEmpty(index);

			var user = SyncContext.Provider.GetValueOrDefault<string>("login");
			var pass = SyncContext.Provider.GetValueOrDefault<string>("password");
			var proxyStr = SyncContext.Provider.GetValueOrDefault<string>("proxy");
			Uri.TryCreate(proxyStr, UriKind.Absolute, out var proxyUrl);

			if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
			{
				throw new ArgumentException("Login and password information must be set!");
			}

			var healthData = new HealthData
			{
				Source = UniqueId,
				Start = start,
				End = end,
				Index = index,
				ExtraData = []
			};

			// start = new DateTimeOffset(start.Date, start.Offset);
			// end = new DateTimeOffset(end.Date, end.Offset);
			var cacheKey = $"{user}:oauth_token";

			try
			{
				// Create or get the OAuth token from the cache
				var oAuthToken = cache.CreateOrGet<OAuth2Token>(cacheKey);

				// Create the HTTP client and Garmin client
				var httpClient = new HttpClient(new SocketsHttpHandler()
				{
					Proxy = proxyUrl != null ? new WebProxy(proxyUrl) : null
				});
				var client = new GarminClient(httpClient, new BasicAuthParameters(user, pass), oAuthToken);


				// Fetch the user profile
				var profile = await client.GetAndDeserialize<GarminSocialProfile>("/userprofile-service/socialProfile", token);

				// Logic to ensure the requested end time does not exceed the last sync time
				var lastUsed = await client.GetAndDeserialize<GarminDeviceLastUsed>("/device-service/deviceservice/mylastused", token);
				if (lastUsed.LastUsedDeviceUploadTime.HasValue)
				{
					var lastSyncTime = DateTimeOffset.FromUnixTimeMilliseconds(lastUsed.LastUsedDeviceUploadTime.Value);
					if (lastSyncTime < end)
					{
						end = lastSyncTime;
						if (end < start)
						{
							start = end;
						}
						Logger.LogWarning("The requested sync end time is later than the last sync time. Adjusting end time to match the last sync.");
					}
				}

				// Update the cache if the OAuth token has changed
				UpdateCacheIfTokenChanged(cacheKey, client.ActiveToken);

				// Collect health data
				await CollectHealthData(healthData, profile, client, start, end, token);

				// Update the cache again if the OAuth token has changed during data collection
				UpdateCacheIfTokenChanged(cacheKey, client.ActiveToken);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error collecting health data from Garmin Connect.");
				throw;
			}

			return healthData;
		}


		private void UpdateCacheIfTokenChanged(string cacheKey, OAuth2Token? newToken)
		{
			var currentToken = cache.CreateOrGet<OAuth2Token>(cacheKey);
			if (currentToken?.AccessToken != newToken?.AccessToken)
			{
				cache.Update(cacheKey, newToken, newToken?.ExpiresIn > 0 ? TimeSpan.FromSeconds(newToken.ExpiresIn) : null);
			}
		}

		private async Task CollectHealthData(HealthData healthData, GarminSocialProfile profile, GarminClient client, DateTimeOffset start, DateTimeOffset end, CancellationToken token)
		{

			var tasks = new List<Task>
			{
				converter.CollectAndConvertDataAsync(
					() => client.GetAndDeserialize<GarminHr>(
						$"/wellness-service/wellness/dailyHeartRate/{profile.DisplayName}?date={start.Date:yyyy-MM-dd}", token),
					healthData,
					token),

				converter.CollectAndConvertDataAsync(
					() => client.GetAndDeserialize<GarminStepsData[]>(
						$"/wellness-service/wellness/dailySummaryChart/{profile.DisplayName}?date={start.Date:yyyy-MM-dd}", token),
					healthData,
					token),

				converter.CollectAndConvertDataAsync(
					() => client.GetAndDeserialize<GarminReportHrvStatus>(
						$"/hrv-service/hrv/daily/{start.Date:yyyy-MM-dd}/{end.Date:yyyy-MM-dd}", token),
					healthData,
					token),

				converter.CollectAndConvertDataAsync(
					() => client.GetAndDeserialize<GarminStress>(
						$"/wellness-service/wellness/dailyStress/{start.Date:yyyy-MM-dd}", token),
					healthData,
					token),

				converter.CollectAndConvertDataAsync(
					() => client.GetAndDeserialize<GarminRespiration>(
						$"/wellness-service/wellness/daily/respiration/{start.Date:yyyy-MM-dd}", token),
					healthData,
					token),

				converter.CollectAndConvertDataAsync(
					() => client.GetAndDeserialize<GarminSpo2>(
						$"/wellness-service/wellness/daily/spo2/{start.Date:yyyy-MM-dd}", token),
					healthData,
					token),

				converter.CollectAndConvertDataAsync(
					() => client.GetAndDeserialize<GarminBodyBatteryData[]>(
						$"/wellness-service/wellness/bodyBattery/reports/daily/?startDate={start.Date:yyyy-MM-dd}&endDate={end.Date:yyyy-MM-dd}", token),
					healthData,
					token),

				converter.CollectAndConvertDataAsync(
					() => client.GetAndDeserialize<GarminSleepData>(
						$"/wellness-service/wellness/dailySleepData/{profile.DisplayName}?date={start.Date:yyyy-MM-dd}", token),
					healthData,
					token),
			};
			await Task.WhenAll(tasks);
		}
	}

}
