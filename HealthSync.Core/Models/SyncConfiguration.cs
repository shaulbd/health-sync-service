using System.Text.Json;

namespace HealthSync.Core.Models
{
	public class SyncConfiguration
	{
		public int MaxSyncSize { get; set; } = 1;
		public List<SyncTask> Sync { get; set; } = new();
	}

	public class SyncTask
	{
		public string Cron { get; set; }
		public required string Index { get; set; }
		public required PluginConfig Input { get; set; }
		public required PluginConfig Output { get; set; }
		public DateTimeOffset? Start { get; set; }
		public DateTimeOffset? End { get; set; }

	}

	public class PluginConfig
	{
		public required string Plugin { get; set; }
		public Dictionary<string, string> Meta { get; set; } = [];
	}
}
