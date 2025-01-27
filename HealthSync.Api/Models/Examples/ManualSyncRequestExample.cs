using HealthSync.Core.Models;
using Swashbuckle.AspNetCore.Filters;

namespace HealthSync.Api.Models.Examples
{
	/// <summary>
	/// Represents a request example for manual synchronization.
	/// </summary>
	public class ManualSyncRequestExample(SyncConfiguration configuration, IWebHostEnvironment env) : IExamplesProvider<ManualSyncRequest>
    {
        /// <summary>
        /// Get the example request.
        /// </summary>
        /// <returns></returns>
        public ManualSyncRequest GetExamples()
        {
            var configIsValid = configuration != null && configuration.Sync != null && configuration.Sync.Count > 0;
            if (configIsValid && env.IsDevelopment())
            {
                return new ManualSyncRequest
                {
					TaskId = configuration.Sync[0].Id,
					Start = DateTimeOffset.UtcNow.AddDays(-1),
                    End = DateTimeOffset.UtcNow
                };
            }
            else
            {
				return new ManualSyncRequest
				{
					TaskId = "example-task-id",
					Start = DateTimeOffset.UtcNow.AddDays(-1),
					End = DateTimeOffset.UtcNow
				};
			}
        }
    }

}
