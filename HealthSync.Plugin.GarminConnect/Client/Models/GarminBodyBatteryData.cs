using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Models;

public record GarminBodyBatteryData
{
    [JsonPropertyName("bodyBatteryValuesArray")]
    public long?[][]? BodyBatteryValuesArray { get; set; }
}