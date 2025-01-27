using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Auth.Models;

public record OAuth1Token
{
    [JsonPropertyName("oauth_token")]
    public string Token { get; set; }

    [JsonPropertyName("oauth_token_secret")]
    public string TokenSecret { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; }

    [JsonPropertyName("mfa_token")]
    public string MFAToken { get; set; }

    [JsonPropertyName("mfa_expiration_timestamp")]
    public DateTime MFAExpirationTimestamp { get; set; }
}