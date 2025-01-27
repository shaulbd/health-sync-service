using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Models;

public record GarminSpo2
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

	[JsonPropertyName("averageSpO2")]
	public double? AverageSpO2 { get; set; }

	[JsonPropertyName("lowestSpO2")]
	public double? LowestSpO2 { get; set; }

	[JsonPropertyName("lastSevenDaysAvgSpO2")]
	public double? LastSevenDaysAvgSpO2 { get; set; }

	[JsonPropertyName("latestSpO2")]
	public double? LatestSpO2 { get; set; }

	[JsonPropertyName("avgSleepSpO2")]
	public double? AvgSleepSpO2 { get; set; }

	[JsonPropertyName("avgTomorrowSleepSpO2")]
	public double? AvgTomorrowSleepSpO2 { get; set; }

	[JsonPropertyName("spO2HourlyAverages")]
	public double?[][]? SpO2HourlyAverages { get; set; }
}