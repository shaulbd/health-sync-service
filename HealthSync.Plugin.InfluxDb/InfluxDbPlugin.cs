using HealthSync.Core;
using HealthSync.Core.Interfaces;
using HealthSync.Core.Models;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Logging;

namespace HealthSync.Plugin.InfluxDb
{
    /// <summary>
    /// Default CTOR.
    /// </summary>
    /// <param name="configuration">Not being used</param>
    public class InfluxDbPlugin(ILogger<InfluxDbPlugin> logger, SyncContext syncContext) : IRepositoryPlugin
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
			using var client = GetClient();

			var writeApi = client.GetWriteApiAsync();

			if (data.HeartRateSamples != null && data.HeartRateSamples.Any())
			{
				var hrData = new InfluxHeartRateData
				{
					Index = index,
					RestingHeartRate = data.RestingHeartRate,
					MaxHeartRate = data.MaximumHeartRate,
					MinHeartRate = data.MinimumHeartRate
				};
				if (data.Metadata != null && data.Metadata.Count != 0)
				{
					hrData.AvgHeartRate7Days = Convert.ToInt64(data.Metadata["last7DaysAvgRestingHeartRate"]);
				}

				hrData.Timestamp = data.HeartRateSamples.First().Timestamp;
				await writeApi.WriteMeasurementAsync(hrData, WritePrecision.S, cancellationToken: token);
				hrData.Timestamp = data.HeartRateSamples.Last().Timestamp;
				await writeApi.WriteMeasurementAsync(hrData, WritePrecision.S, cancellationToken: token);

				foreach (var item in data.HeartRateSamples)
				{
					if (item.HeartRate > 0)
					{
						var hrSample = new InfluxHeartRateSampleData
						{
							Index = index,
							Timestamp = item.Timestamp,
							HeartRate = item.HeartRate
						};
						await writeApi.WriteMeasurementAsync(hrSample, WritePrecision.S, null, null, token);
					}
				}
			}
			if (data.StepsSamples != null && data.StepsSamples.Count != 0)
			{
				foreach (var stepData in data.StepsSamples)
				{
					var stepsData = new InfluxStepsData
					{
						Index = index,
						Steps = stepData.Steps,
						Timestamp = stepData.End
					};
					await writeApi.WriteMeasurementAsync(stepsData, WritePrecision.S, null, null, token);
				}
			}
		}

		public async Task<DateTimeOffset?> GetLastSyncData(string index, CancellationToken token = default)
		{
			using var client = GetClient();
			var queryApi = client.GetQueryApi();
			var fluxQuery = $"from(bucket: \"{Config.Bucket}\")" +
				$" |> range(start: 0, stop: now())" +
				$" |> filter(fn: (r) => r[\"account_id\"] == \"{index}\")" +
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
				Endpoint = SyncContext.RepositoryMeta.GetValueOrDefault<string>("endpoint"),
				Organization = SyncContext.RepositoryMeta.GetValueOrDefault<string>("org", "NA"),
				Token = SyncContext.RepositoryMeta.GetValueOrDefault<string>("token"),
				Bucket = SyncContext.RepositoryMeta.GetValueOrDefault<string>("bucket")
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
