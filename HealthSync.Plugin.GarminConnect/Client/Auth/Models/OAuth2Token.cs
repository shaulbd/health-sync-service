using System.Text.Json.Serialization;

namespace HealthSync.Plugin.GarminConnect.Client.Auth.Models;

public record OAuth2Token
{
    [JsonPropertyName("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("jti")]
    public string Jti { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token_expires_in")]
    public int RefreshTokenExpiresIn { get; set; }
}