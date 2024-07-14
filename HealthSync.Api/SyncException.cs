using HealthSync.BackgroundServices;

namespace HealthSync.Api
{
    public class SyncResultException(SyncResult syncResult) : Exception
    {
		public SyncResult SyncResult { get; } = syncResult;
	}
}