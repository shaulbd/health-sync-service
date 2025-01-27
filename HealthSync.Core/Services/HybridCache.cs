
using System.Collections.Concurrent;
using System.Text.Json;
using HealthSync.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HealthSync.Core.Services;

public class HybridCache : IHybridCache, IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly IPersistentStorage _persistentStorage;
    private readonly ILogger<HybridCache> _logger;
    private readonly ConcurrentDictionary<string, string> _pendingWrites = new();

    public HybridCache(IMemoryCache memoryCache, IPersistentStorage persistentStorage, ILogger<HybridCache> logger)
    {
        _memoryCache = memoryCache;
        _persistentStorage = persistentStorage;
        _logger = logger;

        InitializeAsync().GetAwaiter().GetResult();
    }

    public async Task InitializeAsync()
    {
        await _persistentStorage.InitializeAsync();
        await LoadFromDatabaseAsync();
    }

    public async Task LoadFromDatabaseAsync()
    {
        try
        {
            var cacheEntries = await _persistentStorage.LoadAllAsync();
            foreach (var entry in cacheEntries)
            {
                if (entry.Value.Expiration == null || entry.Value.Expiration > DateTimeOffset.UtcNow)
                {
                    _memoryCache.Set(entry.Key, entry.Value.Value, entry.Value.Expiration ?? DateTimeOffset.MaxValue);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cache from database.");
        }
    }

    public string CreateOrGet(string fullKey, Func<string> valueFactory, TimeSpan? expiration = null)
    {
        if (_memoryCache.TryGetValue(fullKey, out var cachedValue))
        {
            return cachedValue as string;
        }

        var value = valueFactory();
        var expirationTime = expiration.HasValue ? DateTimeOffset.UtcNow.Add(expiration.Value) : (DateTimeOffset?)null;
        _memoryCache.Set(fullKey, value, expirationTime ?? DateTimeOffset.MaxValue);

        if (value != default)
        {
            _pendingWrites[fullKey] = value;
            _ = PersistKeyAsync(fullKey, value, expirationTime); // Fire and forget for immediate persistence
        }
        return value;
    }

    public string CreateOrGet(string fullKey, string value, TimeSpan? expiration = null)
    {
        return CreateOrGet(fullKey, () => value, expiration);
    }

    public string Get(string fullKey)
    {
        return _memoryCache.TryGetValue(fullKey, out var cachedValue) ? cachedValue as string : null;
    }

    public void Update(string fullKey, string value, TimeSpan? expiration = null)
    {
        var expirationTime = expiration.HasValue ? DateTimeOffset.UtcNow.Add(expiration.Value) : (DateTimeOffset?)null;
        _memoryCache.Set(fullKey, value, expirationTime ?? DateTimeOffset.MaxValue);
        _pendingWrites[fullKey] = value;
        _ = PersistKeyAsync(fullKey, value, expirationTime); // Fire and forget for immediate persistence
    }

    public async Task RemoveAsync(string fullKey)
    {
        _memoryCache.Remove(fullKey);
        _pendingWrites.TryRemove(fullKey, out _);

        try
        {
            await _persistentStorage.RemoveAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache key {fullKey} from database.");
        }
    }

    private async Task PersistKeyAsync(string key, string value, DateTimeOffset? expiration = null)
    {
        try
        {
            await _persistentStorage.SaveAsync(key, value, expiration);
            _pendingWrites.TryRemove(key, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error persisting cache key {key} to disk.");
        }
    }

    public async Task PersistPendingWritesAsync()
    {
        foreach (var kvp in _pendingWrites)
        {
            await PersistKeyAsync(kvp.Key, kvp.Value);
        }
    }

    public void Dispose()
    {
        _persistentStorage?.Dispose();
    }
}

public static class CacheExtensions
{
    public const string DELIMITER = "@@@";

    public static string GetFullyQualifiedKey(string cls, string key)
    {
        return $"{cls}{DELIMITER}{key}";
    }

    public static string AsJsonString<T>(this T value)
    {
        if (value == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Serialize(value);
        }
        catch (JsonException ex)
        {
            return null;
        }
    }

    public static T FromJsonString<T>(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(value);
        }
        catch (JsonException ex)
        {
            return default;
        }
    }
}