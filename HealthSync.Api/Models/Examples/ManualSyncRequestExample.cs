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
            var isDev = env.IsDevelopment();
            var configIsValid = configuration != null && configuration.Sync != null && configuration.Sync.Count > 0;

            if (configIsValid && isDev)
            {
                return new ManualSyncRequest
                {
                    Start = DateTimeOffset.UtcNow.AddDays(-1),
                    End = DateTimeOffset.UtcNow,
                    Index = configuration.Sync[0].Index,
                    Provider = new ManualSyncPlugin
                    {
                        Plugin = configuration.Sync[0].Input.Plugin,
                        Meta = configuration.Sync[0].Input.Meta
                    },
                    Repository = new ManualSyncPlugin
                    {
                        Plugin = configuration.Sync[0].Output.Plugin,
                        Meta = configuration.Sync[0].Output.Meta
                    }
                };
            }
            else
            {
                return new ManualSyncRequest
                {
                    Start = DateTimeOffset.UtcNow.AddDays(-1),
                    End = DateTimeOffset.UtcNow,
                    Index = "user123",
                    Provider = new ManualSyncPlugin
                    {
                        Plugin = "garmin-connect",
                        Meta = new Dictionary<string, string>
                {
                    { "login", "john@gmail.com" },
                    { "password", "mypass123" }
                }
                    },
                    Repository = new ManualSyncPlugin
                    {
                        Plugin = "influx-db",
                        Meta = new Dictionary<string, string>
                {
                    { "token", "myToken" },
                    { "endpoint", "http://influx.domain.com:8086/" },
                    { "bucket", "healthSync" },
                    { "org", "myOrg" }
                }
                    }
                };
            }
        }
    }

}
