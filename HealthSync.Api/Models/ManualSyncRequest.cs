using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HealthSync.Api.Models
{
	/// <summary>
	/// Represents a request for manual synchronization.
	/// </summary>
	public class ManualSyncRequest
	{
		/// <summary>
		/// Gets or sets the start time for the sync operation.
		/// </summary>
		[JsonPropertyName("start")]
		public DateTimeOffset Start { get; set; } = DateTimeOffset.UtcNow.AddDays(-1);

		/// <summary>
		/// Gets or sets the end time for the sync operation.
		/// </summary>
		[JsonPropertyName("end")]
		public DateTimeOffset End { get; set; } = DateTimeOffset.UtcNow;

		/// <summary>
		/// Gets or sets the unique identifier for the sync operation.
		/// </summary>
		[JsonPropertyName("index")]
		[Required]
		public required string Index { get; set; }

		/// <summary>
		/// Gets or sets the provider plugin configuration.
		/// </summary>
		[JsonPropertyName("provider")]
		[Required]
		public required ManualSyncPlugin Provider { get; set; }

		/// <summary>
		/// Gets or sets the repository plugin configuration.
		/// </summary>
		[JsonPropertyName("repository")]
		[Required]
		public required ManualSyncPlugin Repository { get; set; }
	}

	/// <summary>
	/// Represents a plugin configuration for manual synchronization.
	/// </summary>
	public class ManualSyncPlugin
	{
		/// <summary>
		/// Gets or sets the name of the plugin.
		/// </summary>
		[JsonPropertyName("plugin")]
		[Required]
		public required string Plugin { get; set; }

		/// <summary>
		/// Gets or sets additional metadata for the plugin.
		/// </summary>
		[JsonPropertyName("meta")]
		public Dictionary<string, string>? Meta { get; set; }
	}

}
