using HealthSync.Core.Models;
using HealthSync.Core.Services;

namespace HealthSync.Core.Interfaces
{
	public interface IRepositoryPlugin : IHealthPlugin
	{
		Task PushAsync(string index, HealthData data, CancellationToken token = default);
		Task<DateTimeOffset?> GetLastSyncData(string index, CancellationToken token = default);
		Task<bool> GetStatus(CancellationToken token = default);
	}
}
