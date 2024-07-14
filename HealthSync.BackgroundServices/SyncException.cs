namespace HealthSync.BackgroundServices
{
	/// <summary>
	/// Sync exception
	/// </summary>
	public class SyncException(string message, List<Exception> exceptions, int successfulChunks, int totalChunks) : Exception(message)
	{
		/// <summary>
		/// All sync exceptions
		/// </summary>
		public List<Exception> Exceptions { get; } = exceptions;

		/// <summary>
		/// Sucessful sync task chunks
		/// </summary>
		public int SuccessfulChunks { get; } = successfulChunks;

		/// <summary>
		/// Total sync task chunks
		/// </summary>
		public int TotalChunks { get; } = totalChunks;
	}
}
