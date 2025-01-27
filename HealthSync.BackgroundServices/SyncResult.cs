using System.Text.Json.Serialization;

namespace HealthSync.BackgroundServices
{
	/// <summary>
	/// Sync result
	/// </summary>
	public enum SyncResultStatus
	{
		Success,
		PartialSuccess, 
		AlreadyRunning,
		TaskNotFound,
		Error
	}

	/// <summary>
	/// Sync Result object
	/// </summary>
	/// <param name="result">Result</param>
	/// <param name="message">Result message</param>
	/// <param name="errors">Sync errors</param>
	public class SyncResult(SyncResultStatus result, string message = "", List<string>? errors = null)
	{
		public bool IsSuccess => Result == SyncResultStatus.Success;
		public SyncResultStatus Result { get; } = result;
		public string Message { get; } = message;
		public List<string> Errors { get; } = errors ?? [];
	}

}
