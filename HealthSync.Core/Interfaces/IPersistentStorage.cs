namespace HealthSync.Core.Interfaces;

public interface IPersistentStorage : IDisposable
{
    Task InitializeAsync();
    Task SaveAsync(string key, string value, DateTimeOffset? expiration = null);
    Task<string> LoadAsync(string key);
    Task RemoveAsync(string key);
    Task<IEnumerable<KeyValuePair<string, (string Value, DateTimeOffset? Expiration)>>> LoadAllAsync();
}