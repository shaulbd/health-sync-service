using HealthSync.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HealthSync.Core
{
    public static class CoreExtensions
    {
        public static IEnumerable<(DateTimeOffset Start, DateTimeOffset End)> ChunkRange(this DateTimeOffset start, DateTimeOffset end, int maxDaysChunk = 1)
        {
            if (start > end)
            {
                throw new ArgumentException("Start date must be earlier than or equal to end date.");
            }

            while (start < end)
            {
                var chunkEnd = start.Date.AddDays(maxDaysChunk) > end ? end : start.Date.AddDays(maxDaysChunk);
                yield return (start, chunkEnd);
                start = chunkEnd;
            }
        }

        public static SyncConfiguration LoadConfiguration(string filePath)
        {
            if (!Path.IsPathRooted(filePath))
            {
                // Combine with the executing path if the filePath is not a full path
                var executingPath = AppDomain.CurrentDomain.BaseDirectory;
                filePath = Path.Combine(executingPath, filePath);
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = File.ReadAllText(filePath);
            return deserializer.Deserialize<SyncConfiguration>(yaml);
        }

        public static T GetValueOrDefault<T>(this Dictionary<string, string> dictionary, string key, T defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(dictionary);

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (dictionary.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }

                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (InvalidCastException)
                {
                    // If conversion fails, return the default value
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }
}
