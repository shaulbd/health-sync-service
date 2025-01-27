using HealthSync.Core.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace HealthSync.Core.Services;

public class SqlitePersistentStorage(string dbPath, ILogger<SqlitePersistentStorage> logger) : IPersistentStorage
{
    private readonly SqliteConnection _dbConnection = new($"Data Source={dbPath}");
    private readonly ILogger<SqlitePersistentStorage> _logger = logger;

    public async Task InitializeAsync()
    {
        var rootDir = Path.GetDirectoryName(_dbConnection.DataSource);
        if (!Directory.Exists(rootDir))
        {
            Directory.CreateDirectory(rootDir);
        }

        await _dbConnection.OpenAsync();
        var createTableCommand = _dbConnection.CreateCommand();
        createTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS Cache (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL,
                Expiration TEXT
            );";
        await createTableCommand.ExecuteNonQueryAsync();
    }

    public async Task SaveAsync(string key, string value, DateTimeOffset? expiration = null)
    {
        var insertCommand = _dbConnection.CreateCommand();
        insertCommand.CommandText = @"
            INSERT INTO Cache (Key, Value, Expiration)
            VALUES (@Key, @Value, @Expiration)
            ON CONFLICT(Key) DO UPDATE SET
                Value = @Value,
                Expiration = CASE WHEN @Expiration IS NOT NULL THEN @Expiration ELSE Expiration END;";
        insertCommand.Parameters.AddWithValue("@Key", key);
        insertCommand.Parameters.AddWithValue("@Value", value);
        insertCommand.Parameters.AddWithValue("@Expiration", expiration?.ToString("O") ?? (object)DBNull.Value);

        await insertCommand.ExecuteNonQueryAsync();
    }

    public async Task<string> LoadAsync(string key)
    {
        var selectCommand = _dbConnection.CreateCommand();
        selectCommand.CommandText = "SELECT Value, Expiration FROM Cache WHERE Key = @Key";
        selectCommand.Parameters.AddWithValue("@Key", key);
        using var reader = await selectCommand.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var expirationString = reader.IsDBNull(1) ? null : reader.GetString(1);
            if (DateTimeOffset.TryParse(expirationString, out var expiration) && expiration < DateTimeOffset.UtcNow)
            {
                await RemoveAsync(key);
                return null;
            }
            return reader.GetString(0);
        }
        return null;
    }

    public async Task RemoveAsync(string key)
    {
        var deleteCommand = _dbConnection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM Cache WHERE Key = @Key";
        deleteCommand.Parameters.AddWithValue("@Key", key);
        await deleteCommand.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<KeyValuePair<string, (string Value, DateTimeOffset? Expiration)>>> LoadAllAsync()
    {
        var selectCommand = _dbConnection.CreateCommand();
        selectCommand.CommandText = "SELECT Key, Value, Expiration FROM Cache";
        using var reader = await selectCommand.ExecuteReaderAsync();
        var results = new List<KeyValuePair<string, (string, DateTimeOffset?)>>();
        while (await reader.ReadAsync())
        {
            var key = reader.GetString(0);
            var value = reader.GetString(1);
            var expirationString = reader.IsDBNull(2) ? null : reader.GetString(2);
            DateTimeOffset? expiration = expirationString == null ? null : DateTimeOffset.Parse(expirationString);
            results.Add(new KeyValuePair<string, (string, DateTimeOffset?)>(key, (value, expiration)));
        }
        return results;
    }

    public void Dispose()
    {
        _dbConnection?.Dispose();
    }
}