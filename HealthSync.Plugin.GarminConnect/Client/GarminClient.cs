using System.Net;
using System.Text.Json;
using HealthSync.Plugin.GarminConnect.Client.Auth.Exceptions;
using HealthSync.Plugin.GarminConnect.Client.Auth.Interfaces;
using HealthSync.Plugin.GarminConnect.Client.Auth.Models;
using HealthSync.Plugin.GarminConnect.Client.Auth.Services;

namespace HealthSync.Plugin.GarminConnect.Client;

public class GarminClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthParameters _authParameters;
    public OAuth2Token ActiveToken { get; private set; }
    private const int Attempts = 3;
    private const int DelayAfterFailAuth = 300;
    private readonly GarminAuthenticationService _garminAuthenticationService;

    public GarminClient(HttpClient httpClient, IAuthParameters authParameters, OAuth2Token token = null)
    {
        ActiveToken = token;
        _httpClient = httpClient;
        _authParameters = authParameters;
        _garminAuthenticationService = new GarminAuthenticationService(_httpClient, authParameters);
    }

    public async Task ReLoginIfExpired(bool force = false, CancellationToken cancellationToken = default)
    {
        if (force || ActiveToken is null)
        {
            var oAuth2Token = await _garminAuthenticationService.Login(cancellationToken);

            ActiveToken = oAuth2Token;
        }
    }
    public async Task<T> GetAndDeserialize<T>(string url, CancellationToken cancellationToken = default)
    {
        var response = await MakeHttpGet(url, cancellationToken: cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        var jsonBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        using var memoryStream = new MemoryStream(jsonBytes);
        return await JsonSerializer.DeserializeAsync<T>(memoryStream, new JsonSerializerOptions(), cancellationToken);
    }

    public Task<HttpResponseMessage> MakeHttpGet(string url,
        IReadOnlyDictionary<string, string> headers = null, CancellationToken cancellationToken = default) =>
        MakeHttpRequest(url, HttpMethod.Get, headers, null, cancellationToken);

    private async Task<HttpResponseMessage> MakeHttpRequest(string url, HttpMethod method,
        IReadOnlyDictionary<string, string> headers, HttpContent content, CancellationToken cancellationToken)
    {
        var force = false;
        Exception exception = null;

        for (var i = 0; i < Attempts; i++)
        {
            try
            {
                await ReLoginIfExpired(force, cancellationToken);

                var requestUri = new Uri($"{_authParameters.BaseUrl}{url}");
                var httpRequestMessage = new HttpRequestMessage(method, requestUri);

                if (headers != null)
                {
                    foreach (var (key, value) in headers)
                    {
                        httpRequestMessage.Headers.Add(key, value);
                    }
                }

                if (!string.IsNullOrEmpty(_authParameters.Cookies))
                {
                    httpRequestMessage.Headers.Add("cookie", _authParameters.Cookies);
                }
                httpRequestMessage.Headers.Add("authorization", $"Bearer {ActiveToken.AccessToken}");
                httpRequestMessage.Headers.Add("di-backend", "connectapi.garmin.com");
                httpRequestMessage.Content = content;

                var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

                await RaiseForStatus(response, cancellationToken);

                return response;
            }
            catch (GarminRequestException ex)
            {
                exception = ex;
                if (ex.Status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    await Task.Delay(DelayAfterFailAuth, cancellationToken);
                    force = true;
                    continue;
                }
                throw;
            }
        }
        throw new GarminAuthenticationException($"Authentication fail after {Attempts} attempts", exception);
    }

    private static Task RaiseForStatus(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        switch (response.StatusCode)
        {
            case HttpStatusCode.TooManyRequests:
                throw new GarminRateLimitException("Too many requests");
            case HttpStatusCode.NoContent:
            case HttpStatusCode.OK:
                return Task.CompletedTask;
            default:
                {
                    var message = $"{response.RequestMessage?.Method.Method}: {response.RequestMessage?.RequestUri}";
                    throw new GarminRequestException(message, response.StatusCode);
                }
        }
    }
}
