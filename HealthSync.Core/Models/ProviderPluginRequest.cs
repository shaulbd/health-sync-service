using HealthSync.Core.Interfaces;

namespace HealthSync.Core.Models
{
	public class ProviderPluginRequest : IProviderPluginRequest
	{
		public DateTimeOffset Start { get; set; }
		public DateTimeOffset End { get; set; }
		public required string Index { get; set; }
		public Dictionary<string, string> Meta { get; set; }
	}
}
