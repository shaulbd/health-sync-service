namespace HealthSync.Plugin.GarminConnect.Client.Auth.Exceptions;

public class GarminRateLimitException(string message) : GarminAuthenticationException(message)
{
}