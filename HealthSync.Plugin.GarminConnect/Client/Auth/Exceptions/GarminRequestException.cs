using System.Net;

namespace HealthSync.Plugin.GarminConnect.Client.Auth.Exceptions;

public class GarminRequestException(string url, HttpStatusCode status) : Exception(
    $"Request [{url}] failed with status code: {(int)status} ({status}).")
{
    public string Url { get; } = url;

    public HttpStatusCode Status { get; } = status;
}
