using HealthSync.Plugin.GarminConnect.Client.Auth.Interfaces;
using HealthSync.Plugin.GarminConnect.Client.Auth.Models;

namespace HealthSync.Plugin.GarminConnect.Client.Auth;

public class BasicAuthParameters : IAuthParameters
{
    private readonly string _email;
    private readonly string _password;
    public string UserAgent => "GCM-iOS-5.7.2.1";
    public string Domain => "garmin.com";
    public string Cookies { get; set; }
    public string Csrf { get; set; }
    public string BaseUrl => $"https://connect.{Domain}";

    public ConsumerCredentials ConsumerCredentials { get; }

    public BasicAuthParameters(string email, string password, ConsumerCredentials consumerCredentials = null)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException(email);
        }

        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException(password);
        }

        _email = email;
        _password = password;
        ConsumerCredentials = consumerCredentials;
    }

    public IReadOnlyDictionary<string, string> GetHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            {
                "User-Agent",
                UserAgent
            },
            {
                "origin",
                $"https://sso.{Domain}"
            }
        };
        
        if (!string.IsNullOrEmpty(Cookies))
        {
            headers.Add("cookie", Cookies);
        }
        
        return headers;
    }

    public IReadOnlyDictionary<string, string> GetFormParameters()
    {
        var data = new Dictionary<string, string>
        {
            { "embed", "true" },
            { "_csrf", Csrf },
            { "username", _email },
            { "password", _password }
        };
        return data;
    }

    public IReadOnlyDictionary<string, string> GetQueryParameters()
    {
        var queryParams = new Dictionary<string, string>
        {
            { "id", "gauth-widget" },
            { "embedWidget", "true" },
        };

        return queryParams;
    }
}
