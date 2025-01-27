using System;
using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Models;

public record GarminDeviceLastUsed
{
    [JsonPropertyName("lastUsedDeviceUploadTime")]
    public long? LastUsedDeviceUploadTime { get; init; }
}