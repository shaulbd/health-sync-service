namespace HealthSync.Plugin.InfluxDb
{
	public class InfluxConfiguration
	{
		public required string Token { get; init; }

		public required string Endpoint { get; init; }

		public required string Bucket { get; init; }

		public required string Organization { get; init; }
	}
}
