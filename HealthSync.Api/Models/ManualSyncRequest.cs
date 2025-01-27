using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HealthSync.Api.Models
{
	/// <summary>
	/// Represents a request for manual synchronization.
	/// </summary>
	public class ManualSyncRequest : IValidatableObject
	{
		/// <summary>
		/// Gets or sets the start time for the sync operation.
		/// </summary>
		[JsonPropertyName("start")]
		[FromQuery(Name = "start")]
		public DateTimeOffset? Start { get; set; }

		/// <summary>
		/// Gets or sets the end time for the sync operation.
		/// </summary>
		[JsonPropertyName("end")]
		[FromQuery(Name = "end")]
		public DateTimeOffset? End { get; set; }

		/// <summary>
		/// Get or sets the Task ID for the sync operation.
		/// </summary>
		[JsonPropertyName("taskId")]
		[FromRoute(Name = "taskId")]
		public required string TaskId { get; init; }

		/// <summary>
		/// Validate the sync request
		/// </summary>
		/// <param name="validationContext">Validation context.</param>
		/// <returns></returns>
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (Start >= End)
			{
				yield return new ValidationResult("Start date must be earlier than End date.", [nameof(Start), nameof(End)]);
			}

			if (End > DateTimeOffset.UtcNow)
			{
				yield return new ValidationResult("End date cannot be in the future.", [nameof(End)]);
			}
		}
	}
}
