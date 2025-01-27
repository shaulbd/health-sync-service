using HealthSync.Plugin.GarminConnect.Client.Auth.Models;

namespace HealthSync.Plugin.GarminConnect.Client.Auth.Interfaces;

public interface IAuthParameters
{
    string UserAgent { get; }
    string Domain { get; }
    string Cookies { get; set; }
    string Csrf { get; set; }

    string BaseUrl { get; }
    
    ConsumerCredentials ConsumerCredentials { get; }

    IReadOnlyDictionary<string, string> GetHeaders();

    IReadOnlyDictionary<string, string> GetFormParameters();

    IReadOnlyDictionary<string, string> GetQueryParameters();
}
