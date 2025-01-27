using HealthSync.Core.Interfaces;
using HealthSync.Core.Models;

namespace HealthSync.Core.Shared
{
	public abstract class HealthDataConverter<TData> : IHealthDataConverter
	{
        public abstract void ConvertAndAdd(HealthData healthData, TData sourceData);
    }
}
