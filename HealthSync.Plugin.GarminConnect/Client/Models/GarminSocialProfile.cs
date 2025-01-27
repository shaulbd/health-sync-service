using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Models;

public record GarminSocialProfile
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("profileId")]
    public long ProfileId { get; init; }

    [JsonPropertyName("garminGUID")]
    public string GarminGuid { get; init; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; }
}