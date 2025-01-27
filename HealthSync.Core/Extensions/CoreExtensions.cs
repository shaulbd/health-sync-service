using System.Runtime.InteropServices;
using HealthSync.Core.Models;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HealthSync.Core.Extensions;


public enum PathNormalizer
{
	Execution, // Relative to the executing assembly
	UserConfig, // User-specific configuration files
	UserData, // User-specific data files
	UserCache, // User-specific cache files
}

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

	public static string NormalizePath(this string path, PathNormalizer pathNormalizer = PathNormalizer.UserConfig)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("Path cannot be null or empty.", nameof(path));
		}

		// If the path is already rooted, return it as-is
		if (Path.IsPathRooted(path))
		{
			return path;
		}

		// Normalize based on the specified type
		switch (pathNormalizer)
		{
			case PathNormalizer.Execution:
				// Combine with the executing assembly's directory
				var executingPath = AppDomain.CurrentDomain.BaseDirectory;
				return Path.Combine(executingPath, path);

			case PathNormalizer.UserConfig:
				// User-specific configuration files
				return GetUserSpecificPath(path, "config");

			case PathNormalizer.UserData:
				// User-specific data files
				return GetUserSpecificPath(path, "data");

			case PathNormalizer.UserCache:
				// User-specific cache files
				return GetUserSpecificPath(path, "cache");

			default:
				throw new ArgumentOutOfRangeException(nameof(pathNormalizer), "Invalid path normalizer type.");
		}
	}

	private static string GetUserSpecificPath(string path, string type)
	{
		var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

		if (isLinux)
		{
			// On Linux, use XDG-compliant paths
			string baseDir;
			switch (type)
			{
				case "config":
					baseDir = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ??
							   Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
					break;

				case "data":
					baseDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
							   Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share");
					break;

				case "cache":
					baseDir = Environment.GetEnvironmentVariable("XDG_CACHE_HOME") ??
							   Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
					break;

				default:
					throw new ArgumentException("Invalid type for Linux path normalization.");
			}

			return Path.Combine(baseDir, "health-sync", path);
		}
		else
		{
			// On Windows, use AppData
			var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			return Path.Combine(localAppData, "health-sync", type, path);
		}
	}

	public static SyncConfiguration LoadConfiguration(string filePath)
	{
		var deserializer = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.Build();

		var yaml = File.ReadAllText(filePath);
		var config = deserializer.Deserialize<SyncConfiguration>(yaml);

		ValidateConfiguration(config);

		return config;
	}

	private static void ValidateConfiguration(SyncConfiguration config)
	{
		foreach (var task in config.Sync)
		{
			if (!ConfigValidator.IsValidPluginId(task.Input.Plugin))
			{
				throw new InvalidDataException($"Invalid Input.Plugin value: {task.Input.Plugin}");
			}

			if (!ConfigValidator.IsValidPluginId(task.Output.Plugin))
			{
				throw new InvalidDataException($"Invalid Output.Plugin value: {task.Output.Plugin}");
			}
		}
	}

	public static T? ExtraAs<T>(this HealthEntity entity, string key) where T : struct
	{
		if (entity.ExtraData.TryGetValue(key, out object? value))
		{
			try
			{
				return (T)Convert.ChangeType(value, typeof(T));
			}
			catch (InvalidCastException)
			{
				return default;
			}
			catch (FormatException)
			{
				return default;
			}
		}
		return null;
	}


	public static T GetValueOrDefault<T>(this IReadOnlyDictionary<string, string> dictionary, string key, T defaultValue = default)
	{
		ArgumentNullException.ThrowIfNull(dictionary);
		ArgumentException.ThrowIfNullOrEmpty(key);

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

	public static async Task<T> WithRetry<T>(this Func<Task<T>> operation, int retryCount, ILogger logger)
	{
		for (int i = 0; i < retryCount; i++)
		{
			try
			{
				return await operation();
			}
			catch (Exception ex)
			{
				if (i == retryCount - 1) throw;
				logger.LogWarning(ex, $"Operation failed. Retrying {i + 1}/{retryCount}");
				await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
			}
		}
		throw new Exception("This line should never be reached");
	}

	public static async Task WithRetry(this Func<Task> operation, int retryCount, ILogger logger)
	{
		for (int i = 0; i < retryCount; i++)
		{
			try
			{
				await operation();
				return;
			}
			catch (Exception ex)
			{
				if (i == retryCount - 1) throw;
				logger.LogWarning(ex, $"Operation failed. Retrying {i + 1}/{retryCount}");
				await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
			}
		}
	}

	public static DateTimeOffset NoFractions(this DateTimeOffset dateTimeOffset)
	{
		return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute, dateTimeOffset.Second, dateTimeOffset.Offset);
	}

	public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime, TimeZoneInfo timeZone)
	{
		// Convert DateTime to UTC
		DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);

		// Create a DateTimeOffset using the UTC DateTime and the specified time zone
		return new DateTimeOffset(utcDateTime, TimeSpan.Zero);
	}


	public static void SafeSetValue<T>(this HealthEntity healthEntity, string key, T? value) where T : struct
	{
		if (value.HasValue)
		{
			healthEntity[key] = value.Value;
		}
	}
}
