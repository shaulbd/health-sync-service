using HealthSync.Core.Models;
using Swashbuckle.AspNetCore.Filters;

namespace HealthSync.Api.Models.Examples
{
	public class LastSyncRequestExample(SyncConfiguration configuration, IWebHostEnvironment env) : IExamplesProvider<LastSyncRequest>
	{
		public LastSyncRequest GetExamples()
		{
			var configIsValid = configuration != null && configuration.Sync != null && configuration.Sync.Count > 0;
			if (configIsValid && env.IsDevelopment())
			{
				return new LastSyncRequest
				{
					TaskId = configuration.Sync[0].Id
				};
			}
			else
			{
				return new LastSyncRequest
				{
					TaskId = "example-task-id"
				};
			}
		}
	}

}
