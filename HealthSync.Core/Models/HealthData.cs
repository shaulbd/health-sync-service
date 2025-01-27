namespace HealthSync.Core.Models
{
	public class HealthEntity
	{
		public DateTimeOffset? Start { get; set; }
		public DateTimeOffset? End { get; set; }

		public bool RangeAvailable => Start.HasValue && End.HasValue;

		public Dictionary<string, object> ExtraData { get; set; } = [];

		public object? this[string key]
		{
			get => ExtraData.TryGetValue(key, out object? value) ? value : null;
			set => _ = !ExtraData.ContainsKey(key) ? ExtraData.TryAdd(key, value) : ExtraData[key] = value;
		}
	}

	public class SampleHealthEntity<T> : HealthEntity
	{
		public required T? Value { get; init; } = default;
	}

	public class HealthData : HealthEntity
	{
		public required string Source { get; init; }
		public HeartRateData? HeartRate { get; set; }
		public StepsData? Steps { get; set; }
		public SleepData? Sleep { get; set; }
		public BodyEnergyData? BodyEnergy { get; set; }
		public StressData? Stress { get; set; }
		public HrvData? HrvData { get; set; }
		public RespirationData? Respiration { get; set; }
		public Spo2Data? Spo2 { get; set; }
		public required string Index { get; init; }
	}

	public class SleepData : HealthEntity
	{
		public TimeSpan? TotalSleep { get; set; }
		public TimeSpan? DeepSleep { get; set; }
		public TimeSpan? LightSleep { get; set; }
		public TimeSpan? RemSleep { get; set; }
		public TimeSpan? Awake { get; set; }
		public TimeSpan? Nap { get; set; }

		public long? Score { get; set; }
		public List<SleepLevelSample> LevelSamples { get; init; } = [];
		public List<SleepMovementSample> MovementSamples { get; set; } = [];
		public List<SleepStressSample> StressSamples { get; set; } = [];
	}

	public class HrvData : HealthEntity
	{
		public List<HrvSample> Samples { get; set; } = [];
	}

	public class HrvSample : SampleHealthEntity<int>
	{
	}

	public class BodyEnergyData : HealthEntity
	{
		public List<BodyEnergySample> Samples { get; set; } = [];
	}

	public class BodyEnergySample : SampleHealthEntity<long>
	{
	}

	public class StepsData : HealthEntity
	{
		public List<StepsSample> Samples { get; init; } = [];
	}

	public class RespirationData : HealthEntity
	{
		public List<RespirationSample> Samples { get; init; } = [];
	}

	public class Spo2Data : HealthEntity
	{
		public List<Spo2Sample> Samples { get; init; } = [];
	}

	public class StressData : HealthEntity
	{
		public List<StressSample> Samples { get; init; } = [];
	}

	public class HeartRateData : HealthEntity
	{
		public List<HeartRateSample> Samples { get; init; } = [];
	}

	public class RespirationSample : SampleHealthEntity<long>
	{
	}

	public class HeartRateSample : SampleHealthEntity<long>
	{
	}

	public class StressSample : SampleHealthEntity<long>
	{
	}

	public class StepsSample : SampleHealthEntity<long>
	{
	}

	public class Spo2Sample : SampleHealthEntity<long>
	{
	}

	public class SleepLevelSample : SampleHealthEntity<SleepStage>
	{
	}

	public class SleepMovementSample : SampleHealthEntity<double>
	{
	}


	public class SleepStressSample : SampleHealthEntity<long>
	{
	}

	public enum SleepStage
	{
		Unknown = 0,
		Deep = 1,
		Light = 2,
		REM = 3,
		Awake = 4
	}
}
