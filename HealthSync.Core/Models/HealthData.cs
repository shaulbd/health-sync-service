namespace HealthSync.Core.Models
{
	public class HealthData
    {
        public List<HeartRateSample> HeartRateSamples { get; set; } = new();
		public List<StepsSample> StepsSamples { get; set; } = new();

		public long? MaximumHeartRate { get; set; }
        public long? MinimumHeartRate { get; set; }
        public long? RestingHeartRate { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = [];
		public required DateTimeOffset StartTime { get; set; }
		public required DateTimeOffset EndTime { get; set; }
		public required string Index { get; set; }
	}

	public class HeartRateSample
    {
        public required DateTimeOffset Timestamp { get; set; }
        public required long HeartRate { get; set; }
    }

	public class StepsSample
	{
		public required long Steps { get; set; }
		public required DateTimeOffset Start { get; set; }
		public required DateTimeOffset End { get; set; }
	}
}
