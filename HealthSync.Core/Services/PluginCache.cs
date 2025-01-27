using HealthSync.Core.Interfaces;

namespace HealthSync.Core.Services;

public class PluginCache<TPlugin>(IHybridCache hybridCache) : IPluginCache<TPlugin> where TPlugin : IHealthPlugin
{
    public string PrefixKey => typeof(TPlugin).Name;

    public T CreateOrGet<T>(string key, T value = default, TimeSpan? expiration = null)
    {
        var fullKey = CacheExtensions.GetFullyQualifiedKey(PrefixKey, key);
        return hybridCache.CreateOrGet(fullKey, () => value.AsJsonString(), expiration).FromJsonString<T>();
    }

    public void Update<T>(string key, T value, TimeSpan? expiration = null)
    {
        var fullKey = CacheExtensions.GetFullyQualifiedKey(PrefixKey, key);
        hybridCache.Update(fullKey, value.AsJsonString(), expiration);
    }

    public async Task Remove(string key)
    {
        var fullKey = CacheExtensions.GetFullyQualifiedKey(PrefixKey, key);
        await hybridCache.RemoveAsync(fullKey);
    }
}
