using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Models;

public record GarminStress
{
    [JsonPropertyName("calendarDate")]
    public DateTime? CalendarDate { get; set; }

    [JsonPropertyName("startTimestampGMT")]
    public DateTime? StartTimestampGmt { get; set; }

    [JsonPropertyName("endTimestampGMT")]
    public DateTime? EndTimestampGmt { get; set; }

    [JsonPropertyName("startTimestampLocal")]
    public DateTime? StartTimestampLocal { get; set; }

    [JsonPropertyName("endTimestampLocal")]
    public DateTime? EndTimestampLocal { get; set; }

    [JsonPropertyName("maxStressLevel")]
    public long? MaxStressLevel { get; set; }

    [JsonPropertyName("avgStressLevel")]
    public long? AvgStressLevel { get; set; }

    [JsonPropertyName("stressValuesArray")]
    public long?[][]? StressValuesArray { get; set; }
}