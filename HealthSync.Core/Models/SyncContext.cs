using System.Threading.Tasks;

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
				SyncId = $"{task.Input.Plugin}_{task.Index}_{task.Output.Plugin}",
				ProviderMeta = task.Input.Meta,
				RepositoryMeta = task.Output.Meta
			};
			return syncContext;
		}

		public string SyncId { get; set; }
		public Dictionary<string, string> ProviderMeta { get; set; }
		public Dictionary<string, string> RepositoryMeta { get; set; }
	}
}
