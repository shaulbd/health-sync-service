
namespace HealthSync.Core.Models
{
	public class SyncContext
	{
        private SyncContext()
        {
            
        }

		public static SyncContext Create(SyncTask task)
		{
			var syncContext = new SyncContext
			{
				SyncId = task.Id,
				Provider = task.Input.Meta,
				Repository = task.Output.Meta
			};
			return syncContext;
		}

		public string SyncId { get; init; }
		public IReadOnlyDictionary<string, string> Provider { get; init; }
		public IReadOnlyDictionary<string, string> Repository { get; init; }
	}
}
