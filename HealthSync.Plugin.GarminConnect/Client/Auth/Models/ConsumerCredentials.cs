using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Auth.Models;

public record ConsumerCredentials
{
    [JsonPropertyName("consumer_key")]
    public string ConsumerKey { get; set; }
    
    [JsonPropertyName("consumer_secret")]
    public string ConsumerSecret { get; set;}
}
