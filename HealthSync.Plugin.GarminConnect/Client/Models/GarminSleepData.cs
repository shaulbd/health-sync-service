using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Models;

public record GarminSleepData
{
    [JsonPropertyName("dailySleepDTO")]
    public DailySleepDto DailySleepDto { get; init; }

    [JsonPropertyName("sleepMovement")]
    public Sleep[] SleepMovement { get; init; }

    [JsonPropertyName("sleepLevels")]
    public Sleep[] SleepLevels { get; init; }

    [JsonPropertyName("sleepStress")]
    public SleepStress[] SleepStress { get; init; }
}

public record DailySleepDto
{
    [JsonPropertyName("calendarDate")]
    public DateTimeOffset CalendarDate { get; init; }

    [JsonPropertyName("sleepTimeSeconds")]
    public long? SleepTimeSeconds { get; init; }

    [JsonPropertyName("napTimeSeconds")]
    public long? NapTimeSeconds { get; init; }

    [JsonPropertyName("sleepStartTimestampGMT")]
    public long? SleepStartTimestampGmt { get; init; }

    // [JsonPropertyName("sleepStartTimestampGMT")]
    // public DateTimeOffset? SleepStartTimestampGmtTest { get; init; }

    [JsonPropertyName("sleepEndTimestampGMT")]
    public long? SleepEndTimestampGmt { get; init; }

    [JsonPropertyName("deepSleepSeconds")]
    public long? DeepSleepSeconds { get; init; }

    [JsonPropertyName("lightSleepSeconds")]
    public long? LightSleepSeconds { get; init; }

    [JsonPropertyName("remSleepSeconds")]
    public long? RemSleepSeconds { get; init; }

    [JsonPropertyName("awakeSleepSeconds")]
    public long? AwakeSleepSeconds { get; init; }

    [JsonPropertyName("sleepScores")]
    public SleepScores SleepScores { get; init; }
}

public record SleepScores
{
    [JsonPropertyName("overall")]
    public Overall Overall { get; init; }
}

public record Overall
{
    [JsonPropertyName("value")]
    public long Value { get; init; }
}

public record Sleep
{
    [JsonPropertyName("startGMT")]
    public DateTimeOffset StartGmt { get; init; }

    [JsonPropertyName("endGMT")]
    public DateTimeOffset EndGmt { get; init; }

    [JsonPropertyName("activityLevel")]
    public double ActivityLevel { get; init; }
}

public record SleepStress
{
    [JsonPropertyName("value")]
    public long Value { get; init; }

    [JsonPropertyName("startGMT")]
    public long StartGmt { get; init; }
}