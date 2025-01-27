using HealthSync.Core.Extensions;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Models;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Logging;

namespace HealthSync.Plugin.InfluxDb
{
	/// <summary>
	/// Influx DB repository plugin
	/// </summary>
	/// <param name="configuration">Not being used</param>
	public class InfluxDbPlugin(ILogger<InfluxDbPlugin> logger, SyncContext syncContext, IPluginCache<InfluxDbPlugin> cache) : IRepositoryPlugin
	{
		/// <summary>
		/// Friendly name
		/// </summary>
		public string Name => "InfluxDB";

		/// <summary>
		/// Unique Identifier
		/// </summary>
		public static string UniqueId => "influx-db";
		public InfluxConfiguration Config { get; private set; }
		public ILogger<InfluxDbPlugin> Logger { get; } = logger;
		public SyncContext SyncContext { get; } = syncContext;

		public async Task PushAsync(string index, HealthData data, CancellationToken token = default)
		{
			ArgumentException.ThrowIfNullOrEmpty(index);
			ArgumentNullException.ThrowIfNull(data);

			using var client = GetClient();

			var writeApi = client.GetWriteApiAsync();

			if (data.HeartRate?.Samples?.Count > 0)
			{
				//// Example:
				//// We can add extra data from various sources. we can use Source property to identify the source and process based on that
				//// Try to get specific farmin plugin extra data
				//if (data.Source == "garmin-connect")
				//{
				//	var hrData = new InfluxHeartRateData
				//	{
				//		Index = index,
				//	};

				//	hrData.RestingHeartRate = data.HeartRate.ExtraAs<long>("RestingHeartRate");
				//	hrData.MaxHeartRate = data.HeartRate.ExtraAs<long>("MaximumHeartRate");
				//	hrData.MinHeartRate = data.HeartRate.ExtraAs<long>("MinimumHeartRate");
				//	hrData.AvgHeartRate7Days = data.HeartRate.ExtraAs<long>("Last7DaysAvgRestingHeartRate");

				//	hrData.Timestamp = data.HeartRate.Samples.First().Timestamp;
				//	await writeApi.WriteMeasurementAsync(hrData, WritePrecision.S, cancellationToken: token);
				//	hrData.Timestamp = data.HeartRate.Samples.Last().Timestamp;
				//	await writeApi.WriteMeasurementAsync(hrData, WritePrecision.S, cancellationToken: token);
				//}

				var hr = data.HeartRate.Samples
					.Where(item => item.Value > 0 && item.Start.HasValue)
					.Select(item => new InfluxHeartRateData
					{
						Index = index,
						Timestamp = item.Start.Value,
						Value = item.Value
					})
					.ToList();

				await writeApi.WriteMeasurementsAsync(hr, WritePrecision.S, null, null, token);
			}
			if (data.Steps?.Samples?.Count > 0)
			{
				var steps = data.Steps.Samples
					.Where(item => item.End.HasValue)
					.Select(item => new InfluxStepsData
					{
						Index = index,
						Timestamp = item.End.Value,
						Value = item.Value
					})
					.ToList();

				await writeApi.WriteMeasurementsAsync(steps, WritePrecision.S, null, null, token);
			}

			if (data.Sleep?.LevelSamples?.Count > 0)
			{
				var sleepStages = data.Sleep.LevelSamples
					.Where(sleepData => sleepData.RangeAvailable)
					.SelectMany(sleepData => new[]
					{
						new InfluxSleepStageData
						{
							Index = index,
							FriendlyName = sleepData.Value.ToString(),
							Value = (int)sleepData.Value,
							Timestamp = sleepData.Start.Value
						},
						new InfluxSleepStageData
						{
							Index = index,
							FriendlyName = sleepData.Value.ToString(),
							Value = (int)sleepData.Value,
							Timestamp = sleepData.End.Value
						}
					})
					.ToList();

				await writeApi.WriteMeasurementsAsync(sleepStages, WritePrecision.S, null, null, token);

				// End the sleep
				await writeApi.WriteMeasurementAsync(new InfluxSleepStageData
				{
					Index = index,
					Value = -1,
					FriendlyName = "NOSLEEP",
					Timestamp = data.Sleep.LevelSamples.Last().End.Value.AddSeconds(1)
				}, WritePrecision.S, null, null, token);
			}

			if (data.Sleep?.MovementSamples?.Count > 0)
			{
				var movements = data.Sleep.MovementSamples
					.Where(sleepData => sleepData.RangeAvailable)
					.SelectMany(sleepData => new[]
					{
						new InfluxSleepMovementData
						{
							Index = index,
							Value = (int)sleepData.Value,
							Timestamp = sleepData.Start.Value
						},
						new InfluxSleepMovementData
						{
							Index = index,
							Value = (int)sleepData.Value,
							Timestamp = sleepData.End.Value
						}
					})
					.ToList();

				await writeApi.WriteMeasurementsAsync(movements, WritePrecision.S, null, null, token);
			}

			if (data.HrvData?.Samples?.Count > 0)
			{
				var hrv = data.HrvData.Samples
					.Where(item => item.Start.HasValue)
					.Select(item => new InfluxHrvData
					{
						Index = index,
						Timestamp = item.Start.Value,
						Value = item.Value
					})
					.ToList();

				await writeApi.WriteMeasurementsAsync(hrv, WritePrecision.S, null, null, token);
			}

			if (data.BodyEnergy?.Samples?.Count > 0)
			{
				var battery = data.BodyEnergy.Samples
					.Where(item => item.Start.HasValue)
					.Select(item => new InfluxBodyBatteryData
					{
						Index = index,
						Timestamp = item.Start.Value,
						Value = (int)item.Value
					})
					.ToList();

				await writeApi.WriteMeasurementsAsync(battery, WritePrecision.S, null, null, token);
			}

			if (data.Spo2?.Samples?.Count > 0)
			{
				var spo2 = data.Spo2.Samples
					.Where(item => item.Start.HasValue)
					.Select(item => new InfluxSpo2Data
					{
						Index = index,
						Timestamp = item.Start.Value,
						Value = item.Value
					})
				.ToList();

				await writeApi.WriteMeasurementsAsync(spo2, WritePrecision.S, null, null, token);
			}

			if (data.Stress?.Samples?.Count > 0)
			{
				var stress = data.Stress.Samples
					.Where(item => item.Start.HasValue)
					.Select(item => new InfluxStressData
					{
						Index = index,
						Timestamp = item.Start.Value,
						Value = item.Value
					})
				.ToList();
				await writeApi.WriteMeasurementsAsync(stress, WritePrecision.S, null, null, token);
			}

			if (data.Respiration?.Samples?.Count > 0)
			{
				var respiration = data.Respiration.Samples
					.Where(item => item.Start.HasValue)
					.Select(item => new InfluxRespirationData
					{
						Index = index,
						Timestamp = item.Start.Value,
						Value = item.Value
					})
				.ToList();
				await writeApi.WriteMeasurementsAsync(respiration, WritePrecision.S, null, null, token);
			}
		}

		public async Task<DateTimeOffset?> GetLastSyncData(string index, CancellationToken token = default)
		{
			ArgumentException.ThrowIfNullOrEmpty(index);

			using var client = GetClient();
			var queryApi = client.GetQueryApi();
			var fluxQuery = $"from(bucket: \"{Config.Bucket}\")" +
				$" |> range(start: 0, stop: now())" +
				$" |> filter(fn: (r) => r[\"index\"] == \"{index}\")" +
				$" |> last()";

			var table = await queryApi.QueryAsync(fluxQuery, null, token);
			if (table?.Count > 0)
			{
				var row = table.LastOrDefault()?.Records.LastOrDefault();
				var timestamp = row?.GetTime();
				if (timestamp.HasValue)
				{
					return timestamp.Value.ToDateTimeOffset();
				}
			}
			return null;
		}

		public async Task<bool> GetStatus(CancellationToken token = default)
		{
			using var client = GetClient();
			return await client.PingAsync();
		}

		private InfluxDBClient GetClient()
		{
			Config ??= new InfluxConfiguration
			{
				Endpoint = SyncContext.Repository.GetValueOrDefault<string>("endpoint"),
				Organization = SyncContext.Repository.GetValueOrDefault<string>("org", "NA"),
				Token = SyncContext.Repository.GetValueOrDefault<string>("token"),
				Bucket = SyncContext.Repository.GetValueOrDefault<string>("bucket")
			};

			var client = new InfluxDBClient(new InfluxDBClientOptions(Config.Endpoint)
			{
				Org = Config.Organization,
				Token = Config.Token,
				Bucket = Config.Bucket
			});
			return client;
		}
	}
}
