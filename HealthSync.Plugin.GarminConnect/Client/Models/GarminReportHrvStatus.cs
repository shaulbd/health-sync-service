using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Models;

public record GarminReportHrvStatus
{
    [JsonPropertyName("hrvSummaries")]
    public GarminHrvSummary[] HrvSummaries { get; init; }
}

public record GarminHrvSummary
{
    [JsonPropertyName("calendarDate")]
    public DateOnly CalendarDate { get; init; }

    [JsonPropertyName("weeklyAvg")]
    public int WeeklyAvg { get; init; }

    [JsonPropertyName("lastNightAvg")]
    public int LastNightAvg { get; init; }

    [JsonPropertyName("lastNight5MinHigh")]
    public int LastNight5MinHigh { get; init; }

    [JsonPropertyName("createTimeStamp")]
    public DateTime CreateTimeStamp { get; init; }
}