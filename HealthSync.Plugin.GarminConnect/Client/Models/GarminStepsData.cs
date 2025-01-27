using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Models;

public record GarminStepsData
{
    [JsonPropertyName("startGMT")]
    public DateTime StartGmt { get; init; }

    [JsonPropertyName("endGMT")]
    public DateTime EndGmt { get; init; }

    [JsonPropertyName("steps")]
    public long Steps { get; init; }

    [JsonPropertyName("primaryActivityLevel")]
    public string PrimaryActivityLevel { get; init; }
    
    [JsonPropertyName("activityLevelConstant")]
    public bool ActivityLevelConstant { get; init; }
}