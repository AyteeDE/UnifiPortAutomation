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
    private async static Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod, string url, List<(string, string)> headers, HttpContent? content = null)
    {
        HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);
        request.Content = content;
        foreach(var header in headers)
        {
            request.Headers.Add(header.Item1, header.Item2);
        }
        
        var client = new CustomHttpClient();
        try
        {
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch(HttpRequestException e)
        {
            switch(e.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new HttpRequestException("Unauthorized access to Unifi API. Please check your username, password and permissions.");
                case HttpStatusCode.Forbidden:
                    throw new HttpRequestException("Unauthorized access to Unifi API. Please check your username, password and permissions.");
                default: 
                    throw new HttpRequestException("Unifi Host is not reachable under the given IP.", e);
            }
        }
        catch(Exception e)
        {
            throw new Exception(e.Message, e);
        }
    }
    private async static Task<(string, string)> LoginAsync(string username, string password)
    {
        (string?, string?) loginTokens = (null, null);
        string url = $"https://{_host}/api/auth/login";
        HttpContent content = new StringContent($"{{\"username\":\"{username}\",\"password\":\"{password}\"}}", Encoding.UTF8, "application/json");
        HttpResponseMessage response = await SendAsync(HttpMethod.Post, url, new List<(string, string)>(), content);

        var cookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        loginTokens.Item1 = cookieHeader.Substring(0, cookieHeader.IndexOf(';'));;
        loginTokens.Item2 = response.Headers.GetValues("X-CSRF-Token").FirstOrDefault();
        
        return loginTokens;
    }
    private async Task LogoutAsync()
    {
        string url = $"https://{_host}/api/auth/logout";
        List<(string, string)> headers = new List<(string, string)>{("Cookie", _cookie), ("X-CSRF-Token", _csrfToken)};
        await SendAsync(HttpMethod.Post, url, headers);

        _cookie = null;
        _csrfToken = null;
    }
    public async Task<UnifiAPIResponse> GetPortForwardingsAsync()
    {
        string url = $"https://{_host}/proxy/network/api/s/default/rest/portforward";
        List<(string, string)> headers = new List<(string, string)>{("Cookie", _cookie)};
        HttpResponseMessage response = await SendAsync(HttpMethod.Get, url, headers);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UnifiAPIResponse>(content);
    }
    public async Task PutPortForwardingAsync(UnifiAPIResponseData forwarding)
    {
        string url = $"https://{_host}/proxy/network/api/s/default/rest/portforward/{forwarding.Id}";
        List<(string, string)> headers = new List<(string, string)>{("Cookie", _cookie), ("X-CSRF-Token", _csrfToken)};
        var content = new StringContent(JsonSerializer.Serialize(forwarding), Encoding.UTF8, "application/json");
        await SendAsync(HttpMethod.Put, url, headers, content);
    }
    public async ValueTask DisposeAsync()
    {
        await LogoutAsync();
    }
}
