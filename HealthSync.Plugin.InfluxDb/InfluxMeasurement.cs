using InfluxDB.Client.Core;

namespace HealthSync.Plugin.InfluxDb
{
	[Measurement("heart_rate")]
	public class InfluxHeartRateSampleData
	{
		[Column("account_id", IsTag = true)]
		public string Index { get; set; }

		[Column("heart_rate")]
		public long HeartRate { get; set; }

		[Column(IsTimestamp = true)]
		public DateTimeOffset Timestamp { get; set; }
	}

	[Measurement("heart_rate_extra")]
	public class InfluxHeartRateData
	{
		[Column("account_id", IsTag = true)]
		public string Index { get; set; }

		[Column("max_heart_rate")]
		public long? MaxHeartRate { get; set; }

		[Column("min_heart_rate")]
		public long? MinHeartRate { get; set; }

		[Column("resting_heart_rate")]
		public long? RestingHeartRate { get; set; }

		[Column("avg_heart_rate_7days")]
		public long? AvgHeartRate7Days { get; set; }

		[Column(IsTimestamp = true)]
		public DateTimeOffset Timestamp { get; set; }
	}

	[Measurement("steps")]
	public class InfluxStepsData
	{
		[Column("account_id", IsTag = true)]
		public string Index { get; set; }

		[Column("steps")]
		public long Steps { get; set; }

		[Column(IsTimestamp = true)]
		public DateTimeOffset Timestamp { get; set; }
	}
}
