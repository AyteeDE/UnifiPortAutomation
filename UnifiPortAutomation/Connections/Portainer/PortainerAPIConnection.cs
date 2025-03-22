using System;
using System.Net.Http.Headers;
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
        var response = await client.GetAsync(url);
        if(response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var containers = JsonSerializer.Deserialize<List<PortainerContainerAPIResponse>>(responseContent);
            return containers;
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
