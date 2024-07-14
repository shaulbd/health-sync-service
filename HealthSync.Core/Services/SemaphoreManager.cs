using System.Collections.Concurrent;

namespace HealthSync.Core.Services
{
	public class SemaphoreManager
	{
		private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

		public SemaphoreSlim GetSemaphore(string syncId)
		{
			return _semaphores.GetOrAdd(syncId, _ => new SemaphoreSlim(1, 1));
		}

		public void ReleaseSemaphore(string syncId)
		{
			if (_semaphores.TryRemove(syncId, out var semaphore))
			{
				semaphore.Dispose();
			}
		}
	}
}
