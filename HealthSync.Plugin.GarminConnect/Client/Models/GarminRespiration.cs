using System.Text.Json.Serialization;
namespace HealthSync.Plugin.GarminConnect.Client.Models;

public record GarminRespiration
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

    [JsonPropertyName("lowestRespirationValue")]
    public double? LowestRespirationValue { get; set; }

    [JsonPropertyName("highestRespirationValue")]
    public double? HighestRespirationValue { get; set; }

    [JsonPropertyName("avgWakingRespirationValue")]
    public double? AvgWakingRespirationValue { get; set; }

    [JsonPropertyName("avgSleepRespirationValue")]
    public double? AvgSleepRespirationValue { get; set; }

    [JsonPropertyName("avgTomorrowSleepRespirationValue")]
    public double? AvgTomorrowSleepRespirationValue { get; set; }

    [JsonPropertyName("respirationValuesArray")]
    public double?[][]? RespirationValuesArray { get; set; }
}