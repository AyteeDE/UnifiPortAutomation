using System;
using System.Net;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Text.Json;

namespace UnifiPortAutomation.Connections.Portainer;

public class PortainerAPIConnection
{
    private static string _host;
    private string _accessToken;
    private PortainerAPIConnection(string token)
    {
        _accessToken = token;
    }
    public static async Task<PortainerAPIConnection> CreateAsync(string host, string token)
    {
        _host = host;
        PortainerAPIConnection connection = new PortainerAPIConnection(token);
        return connection;
    }
    public async Task<List<PortainerContainerAPIResponse>> GetRunningContainers(int environmentId)
    {
        string url = $"https://{_host}/api/endpoints/{environmentId}/docker/containers/json";
        var client = CreateClient();
        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var containers = JsonSerializer.Deserialize<List<PortainerContainerAPIResponse>>(responseContent);
            return containers;
        }
        catch(HttpRequestException e)
        {
            switch(e.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new HttpRequestException("Environment not found. Please check your environmentId.");
                case HttpStatusCode.Unauthorized:
                    throw new HttpRequestException("Unauthorized access to Portainer API. Please check your access token.");
                default: 
                    throw new HttpRequestException("Portainer Host is not reachable under the given IP or port.", e);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }

        return new List<PortainerContainerAPIResponse>();
    }
    private HttpClient CreateClient()
    {
        var client = new CustomHttpClient();
        client.DefaultRequestHeaders.Add("X-API-KEY", _accessToken);
        return client;
    }
}
