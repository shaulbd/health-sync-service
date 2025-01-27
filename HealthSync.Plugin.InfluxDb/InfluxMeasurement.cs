using InfluxDB.Client.Core;

namespace HealthSync.Plugin.InfluxDb
{
	public class IndexedMeasurementData<T>
	{
		[Column("index", IsTag = true)]
		public required string Index { get; set; }

		[Column(IsTimestamp = true)]
		public DateTimeOffset Timestamp { get; set; }

		[Column("value")]
		public required T Value { get; set; }
	}

	[Measurement("heart_rate")]
	public class InfluxHeartRateData : IndexedMeasurementData<long>
	{
	}

	[Measurement("steps")]
	public class InfluxStepsData : IndexedMeasurementData<long>
	{
	}

	[Measurement("sleep_stages")]
	public class InfluxSleepStageData : IndexedMeasurementData<int>
	{
		[Column("friendly_name", IsTag = true)]
		public string FriendlyName { get; set; }
	}

	[Measurement("sleep_movements")]
	public class InfluxSleepMovementData : IndexedMeasurementData<double>
	{
	}

	[Measurement("hrv")]
	public class InfluxHrvData : IndexedMeasurementData<int>
	{
	}

	[Measurement("body_battery")]
	public class InfluxBodyBatteryData : IndexedMeasurementData<int>
	{
	}

	[Measurement("stress")]
	public class InfluxStressData : IndexedMeasurementData<long>
	{
	}

	[Measurement("respiration")]
	public class InfluxRespirationData : IndexedMeasurementData<long>
	{
	}

	[Measurement("spo2")]
	public class InfluxSpo2Data : IndexedMeasurementData<long>
	{
	}
}
