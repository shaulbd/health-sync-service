using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using HealthSync.Plugin.GarminConnect.Client.Auth.Exceptions;
using HealthSync.Plugin.GarminConnect.Client.Auth.Interfaces;
using HealthSync.Plugin.GarminConnect.Client.Auth.Models;
using OAuth;

namespace HealthSync.Plugin.GarminConnect.Client.Auth.Services;

internal class GarminAuthenticationService(HttpClient httpClient, IAuthParameters authParameters)
{
    private readonly IAuthParameters _authParameters = authParameters;
    private readonly HttpClient _httpClient = httpClient;
    private string SsoUrl => $"https://sso.{_authParameters.Domain}/sso";
    private string EmbedUrl => $"{SsoUrl}/embed";
    private string SigninUrl => $"{SsoUrl}/signin";

    public async Task<OAuth2Token> Login(CancellationToken cancellationToken)
    {
        try
        {
            _authParameters.Cookies = await GetCookies(cancellationToken);
            _authParameters.Csrf = await GetCsrfToken(cancellationToken);

            var ticket = await GetOAuthTicket(cancellationToken);
            var consumerCredentials = _authParameters.ConsumerCredentials ?? await GetConsumerCredentials(cancellationToken);
            var auth1Token = await GetOAuth1Token(ticket, consumerCredentials, cancellationToken);

            return await GetOAuth2TokenAsync(auth1Token, consumerCredentials, cancellationToken);
        }
        catch (Exception e)
        {
            throw new GarminAuthenticationException("Garmin authentication failed.", e);
        }
    }

    private async Task<ConsumerCredentials> GetConsumerCredentials(CancellationToken cancellationToken)
    {
        var oauthConsumerUrl = Environment.GetEnvironmentVariable("OAUTH_CONSUMER_URL");
        if (string.IsNullOrWhiteSpace(oauthConsumerUrl))
        {
            oauthConsumerUrl = "https://thegarth.s3.amazonaws.com/oauth_consumer.json";
        }

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, oauthConsumerUrl);
        var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);
        var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<ConsumerCredentials>(content);
    }


    private async Task<string> GetCookies(CancellationToken cancellationToken)
    {
        var queryEmbed = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kv in _authParameters.GetQueryParameters())
        {
            queryEmbed.Add(kv.Key, kv.Value);
        }

        queryEmbed.Add("gauthHost", SsoUrl);

        var requestUriEmbed = $"{EmbedUrl}?{queryEmbed}";

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUriEmbed);
        foreach (var kv in _authParameters.GetHeaders())
        {
            httpRequestMessage.Headers.Add(kv.Key, kv.Value);
        }

        var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

        if (responseMessage.StatusCode != HttpStatusCode.OK)
            throw new GarminAuthenticationException("Failed to fetch cookies from Garmin.");

        var headerCookies = responseMessage.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
        var sb = new StringBuilder();
        foreach (var cookie in headerCookies)
        {
            sb.Append($"{cookie};");
        }

        var cookies = sb.ToString();

        if (string.IsNullOrWhiteSpace(cookies))
            throw new GarminAuthenticationException("Cookies are invalid.");

        return cookies;
    }

    private async Task<string> GetCsrfToken(CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>(_authParameters.GetQueryParameters())
        {
            { "gauthHost", EmbedUrl },
            { "service", EmbedUrl },
            { "source", EmbedUrl },
            { "redirectAfterAccountLoginUrl", EmbedUrl },
            { "redirectAfterAccountCreationUrl", EmbedUrl }
        };

        var queryCsrf = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kv in parameters)
        {
            queryCsrf.Add(kv.Key, kv.Value);
        }

        var requestUriSignin = $"{SigninUrl}?{queryCsrf}";
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUriSignin);
        foreach (var kv in _authParameters.GetHeaders())
        {
            httpRequestMessage.Headers.Add(kv.Key, kv.Value);
        }

        var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

        if (responseMessage.StatusCode != HttpStatusCode.OK)
            throw new GarminAuthenticationException("Failed to fetch csrf token from Garmin.");

        var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        var regexCsrf = new Regex("name=\"_csrf\"\\s+value=\"(.+?)\"",
            RegexOptions.Compiled | RegexOptions.Multiline);

        var match = regexCsrf.Match(content);

        if (!match.Success)
            throw new GarminAuthenticationException("Failed to retrieve CSRF token.");

        var csrf = match.Groups[1].Value;

        if (string.IsNullOrWhiteSpace(csrf))
            throw new GarminAuthenticationException("Invalid CSRF token.");

        return match.Groups[1].Value;
    }


    private async Task<string> GetOAuthTicket(CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>(_authParameters.GetQueryParameters())
        {
            { "gauthHost", EmbedUrl },
            { "service", EmbedUrl },
            { "source", EmbedUrl },
            { "redirectAfterAccountLoginUrl", EmbedUrl },
            { "redirectAfterAccountCreationUrl", EmbedUrl }
        };

        var queryCsrf = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kv in parameters)
        {
            queryCsrf.Add(kv.Key, kv.Value);
        }

        var requestUriSignin = $"{SigninUrl}?{queryCsrf}";
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUriSignin);
        foreach (var kv in _authParameters.GetHeaders())
        {
            httpRequestMessage.Headers.Add(kv.Key, kv.Value);
        }

        httpRequestMessage.Headers.Add("referer", SigninUrl);
        httpRequestMessage.Headers.Add("NK", "NT");
        httpRequestMessage.Content = new FormUrlEncodedContent(_authParameters.GetFormParameters());

        var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);
        var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        if (responseMessage.StatusCode is HttpStatusCode.TooManyRequests)
        {
            throw new GarminRateLimitException("Rate limit exceeded.");
        }

        if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new GarminAuthenticationException("Access forbidden.");
        }

        var regexTicket = new Regex(@"embed\?ticket=([^""]+)""", RegexOptions.Compiled | RegexOptions.Multiline);
        var match = regexTicket.Match(content);

        if (!match.Success)
            throw new GarminAuthenticationException("Failed to retrieve OAuth ticket.");

        var ticket = match.Groups[1].Value;

        if (string.IsNullOrWhiteSpace(ticket))
            throw new GarminAuthenticationException("Invalid OAuth ticket.");

        return ticket;
    }

    private async Task<OAuth1Token> GetOAuth1Token(string ticket, ConsumerCredentials credentials,
        CancellationToken cancellationToken)
    {
        string oauth1Response;
        try
        {
            var oauthClient = OAuthRequest.ForRequestToken(credentials.ConsumerKey, credentials.ConsumerSecret);
            oauthClient.RequestUrl =
                $"https://connectapi.{_authParameters.Domain}/oauth-service/oauth/preauthorized?ticket={ticket}&login-url=https://sso.garmin.com/sso/embed&accepts-mfa-tokens=true";

            var authHeaders = oauthClient.GetAuthorizationHeader();
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, oauthClient.RequestUrl);
            httpRequestMessage.Headers.Add("User-Agent", _authParameters.UserAgent);
            httpRequestMessage.Headers.Add("Authorization", authHeaders);
            var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

            oauth1Response = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception e)
        {
            throw new GarminAuthenticationException("Failed to get OAuth1 token.", e);
        }

        if (string.IsNullOrWhiteSpace(oauth1Response))
        {
            throw new GarminAuthenticationException("OAuth1 Token response is invalid.");
        }

        var queryParams = HttpUtility.ParseQueryString(oauth1Response);

        var oAuthToken = queryParams.Get("oauth_token");
        var oAuthTokenSecret = queryParams.Get("oauth_token_secret");

        if (string.IsNullOrWhiteSpace(oAuthToken))
        {
            throw new GarminAuthenticationException($"OAuth1 token is invalid. Response: {oauth1Response}");
        }

        if (string.IsNullOrWhiteSpace(oAuthTokenSecret))
        {
            throw new GarminAuthenticationException($"OAuth1 token secret is invalid. Response: {oauth1Response}");
        }

        return new OAuth1Token
        {
            Token = oAuthToken,
            TokenSecret = oAuthTokenSecret
        };
    }

    private async Task<OAuth2Token> GetOAuth2TokenAsync(OAuth1Token oAuth1Token, ConsumerCredentials credentials,
        CancellationToken cancellationToken)
    {
        var oauth2Client = OAuthRequest.ForProtectedResource("POST", credentials.ConsumerKey,
            credentials.ConsumerSecret, oAuth1Token.Token, oAuth1Token.TokenSecret);
        oauth2Client.RequestUrl = $"https://connectapi.{_authParameters.Domain}/oauth-service/oauth/exchange/user/2.0";

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, oauth2Client.RequestUrl);
        httpRequestMessage.Headers.Add("User-Agent", _authParameters.UserAgent);
        httpRequestMessage.Headers.Add("Authorization", oauth2Client.GetAuthorizationHeader());

        httpRequestMessage.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>() });
        var responseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

        var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<OAuth2Token>(content);
    }
}