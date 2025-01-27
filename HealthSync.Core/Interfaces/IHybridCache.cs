namespace HealthSync.Core.Interfaces;

public interface IHybridCache
{
    Task InitializeAsync();
    string CreateOrGet(string fullKey, Func<string> valueFactory, TimeSpan? expiration = null);
    string CreateOrGet(string fullKey, string value, TimeSpan? expiration = null);
    string Get(string fullKey);
    void Update(string fullKey, string value, TimeSpan? expiration = null);
    Task RemoveAsync(string fullKey);
    Task LoadFromDatabaseAsync();
    Task PersistPendingWritesAsync();
}