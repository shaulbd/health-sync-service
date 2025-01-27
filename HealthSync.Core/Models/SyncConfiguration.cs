
namespace HealthSync.Core.Models
{
	public class SyncConfiguration
	{
		public int MaxSyncSize { get; set; } = 1;
		public int RetryCount { get; set; } = 3;
		public List<SyncTask> Sync { get; set; } = new();
	}

	public record SyncTask
	{
		public string Cron { get; init; }
		public required string Index { get; init; }
		public required PluginConfig Input { get; init; }
		public required PluginConfig Output { get; init; }
		public DateTimeOffset? Start { get; init; }
		public DateTimeOffset? End { get; init; }
		public string Id => $"{Input.Plugin}_{Index}_{Output.Plugin}";
	}

	public record PluginConfig
	{
		public required string Plugin { get; init; }
		public Dictionary<string, string> Meta { get; init; } = new();
	}
}
