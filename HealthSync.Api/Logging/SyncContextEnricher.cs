using HealthSync.BackgroundServices;
using Serilog.Core;
using Serilog.Events;

namespace HealthSync.Api.Logging
{
    public class SyncContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (LoggerScope.Current != null)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "SyncContextId", LoggerScope.Current.SyncContextId));
            }
        }
    }
}