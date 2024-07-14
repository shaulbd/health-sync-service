using Garmin.Connect;
using Garmin.Connect.Auth;
using HealthSync.Core;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace HealthSync.Plugin.GarminConnect
{

    public class GarminConnectPlugin(ILogger<GarminConnectPlugin> logger, SyncContext syncContext) : IProviderPlugin
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
			[NotNull] DateTimeOffset start,
			[NotNull] DateTimeOffset end,
			[NotNull] string index,
			CancellationToken token = default)
		{
			var user = SyncContext.ProviderMeta.GetValueOrDefault<string>("login");
			var pass = SyncContext.ProviderMeta.GetValueOrDefault<string>("password");

			if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
			{
				throw new ArgumentException("Login and password information must be set!");
			}

			// Garmin API can only provide 24HR span of data so start time will always get data of start + 24hr (End time is ignored)
			HealthData healthData = GarminConverter.Init(index, start.DateTime, start.DateTime.AddHours(24));

			var auth = new BasicAuthParameters(user, pass);
			var client = new GarminConnectClient(new GarminConnectContext(new HttpClient(), auth));

			var hr = await client.GetWellnessHeartRates(start.DateTime, token);
			healthData.AddGarminHeartRate(hr);

			var steps = await client.GetWellnessStepsData(start.DateTime, token);
			healthData.AddGarminSteps(steps);

			return healthData;
		}
	}
}
