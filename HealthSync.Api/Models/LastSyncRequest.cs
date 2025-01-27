using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace HealthSync.Api.Models
{
	/// <summary>
	/// Represents a request for last synchronization timestamp.
	/// </summary>
	public class LastSyncRequest
	{
		/// <summary>
		/// Get or sets the Task ID for the sync operation.
		/// </summary>
		[JsonPropertyName("taskId")]
		[FromRoute(Name = "taskId")]
		public required string TaskId { get; init; }
    }
}
