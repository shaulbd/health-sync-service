using System.Text.Json;

namespace HealthSync.Core.Interfaces
{
	public interface IProviderPluginRequest
	{
		DateTimeOffset Start { get; }
		DateTimeOffset End { get; }
		string Index { get; }
		Dictionary<string, string> Meta { get; }
	}
}
