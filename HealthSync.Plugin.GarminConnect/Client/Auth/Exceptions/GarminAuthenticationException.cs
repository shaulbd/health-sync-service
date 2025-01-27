namespace HealthSync.Plugin.GarminConnect.Client.Auth.Exceptions;

public class GarminAuthenticationException : Exception
{
    public GarminAuthenticationException(string message) : base(message) { }
    public GarminAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}
