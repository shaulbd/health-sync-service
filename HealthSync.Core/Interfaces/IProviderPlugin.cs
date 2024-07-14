using HealthSync.Core.Models;

namespace HealthSync.Core.Interfaces
{
	public interface IProviderPlugin : IHealthPlugin
	{
		Task<HealthData> GetAsync(DateTimeOffset start, DateTimeOffset end, string index, CancellationToken token = default);
	}
}
