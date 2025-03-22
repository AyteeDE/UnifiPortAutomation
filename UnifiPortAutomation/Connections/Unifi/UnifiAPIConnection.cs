using System;
using System.Net;
using System.Text;
using System.Text.Json;

namespace UnifiPortAutomation.Connections.Unifi;

public class UnifiAPIConnection : IAsyncDisposable
{
    private static string _host;
    private string _cookie;
    private string _csrfToken;

    private UnifiAPIConnection(string cookie, string csrfToken)
    {
        _cookie = cookie;
        _csrfToken = csrfToken;
    }
    public static async Task<UnifiAPIConnection> CreateAsync(string username, string password, string host)
    {
        _host = host;
        var loginTokens = await LoginAsync(username, password);
        UnifiAPIConnection connection = new UnifiAPIConnection(loginTokens.Item1, loginTokens.Item2);
        return connection;
    }

    private async static Task<(string, string)> LoginAsync(string username, string password)
    {
        (string?, string?) loginTokens = (null, null);
        string url = $"https://{_host}/api/auth/login";
        var client = new CustomHttpClient();
        HttpContent content = new StringContent($"{{\"username\":\"{username}\",\"password\":\"{password}\"}}", Encoding.UTF8, "application/json");
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync(url, content);
            if(response.IsSuccessStatusCode)
            {
                var cookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
                loginTokens.Item1 = cookieHeader.Substring(0, cookieHeader.IndexOf(';'));;
                loginTokens.Item2 = response.Headers.GetValues("X-CSRF-Token").FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
        return loginTokens;
    }
    private async Task LogoutAsync()
    {
        string url = $"https://{_host}/api/auth/logout";
        var client = new CustomHttpClient();
        client.DefaultRequestHeaders.Add("Cookie", _cookie);
        client.DefaultRequestHeaders.Add("X-CSRF-Token", _csrfToken);
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync(url, null);
            if(response.IsSuccessStatusCode)
            {
                _cookie = null;
                _csrfToken = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    public async Task<UnifiAPIResponse> GetPortForwardingsAsync()
    {
        string url = $"https://{_host}/proxy/network/api/s/default/rest/portforward";
        var client = new CustomHttpClient();
        client.DefaultRequestHeaders.Add("Cookie", _cookie);
        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(url);
            if(response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UnifiAPIResponse>(content);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return new UnifiAPIResponse();
    }
    public async Task<bool> PutPortForwardingAsync(UnifiAPIResponseData forwarding)
    {
        string url = $"https://{_host}/proxy/network/api/s/default/rest/portforward/{forwarding.Id}";
        var client = new CustomHttpClient();
        client.DefaultRequestHeaders.Add("Cookie", _cookie);
        client.DefaultRequestHeaders.Add("X-CSRF-Token", _csrfToken);
        var content = new StringContent(JsonSerializer.Serialize(forwarding), Encoding.UTF8, "application/json");
        HttpResponseMessage response;
        try
        {
            response = await client.PutAsync(url, content);
            if(response.IsSuccessStatusCode)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return false;
    }
    public async ValueTask DisposeAsync()
    {
        await LogoutAsync();
    }
}
