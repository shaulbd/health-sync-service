namespace HealthSync.Core.Interfaces;

public interface IPluginCache<TPlugin>
{
    public string PrefixKey {get;}
    T CreateOrGet<T>(string key, T value=default, TimeSpan? expiration = null);
    void Update<T>(string key, T value, TimeSpan? expiration = null);
    Task Remove(string key);
}