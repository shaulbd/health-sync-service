using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Models;

public record GarminHr
{
    [JsonPropertyName("calendarDate")]
    public DateTime CalendarDate { get; init; }

    [JsonPropertyName("startTimestampGMT")]
    public DateTime StartTimestampGmt { get; init; }

    [JsonPropertyName("endTimestampGMT")]
    public DateTime EndTimestampGmt { get; init; }

    [JsonPropertyName("startTimestampLocal")]
    public DateTime StartTimestampLocal { get; init; }

    [JsonPropertyName("endTimestampLocal")]
    public DateTime EndTimestampLocal { get; init; }

    [JsonPropertyName("maxHeartRate")]
    public long? MaxHeartRate { get; init; }

    [JsonPropertyName("minHeartRate")]
    public long? MinHeartRate { get; init; }

    [JsonPropertyName("restingHeartRate")]
    public long? RestingHeartRate { get; init; }

    [JsonPropertyName("lastSevenDaysAvgRestingHeartRate")]
    public long? LastSevenDaysAvgRestingHeartRate { get; init; }

    [JsonPropertyName("heartRateValues")]
    public long?[][]? HeartRateValues { get; init; }
}