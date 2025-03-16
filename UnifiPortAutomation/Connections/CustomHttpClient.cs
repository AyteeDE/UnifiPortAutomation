using System;

namespace UnifiPortAutomation.Connections;

public class CustomHttpClient : HttpClient
{
    private static HttpClientHandler _clientHandler
    {
        get
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            return clientHandler;
        }
    }
    public CustomHttpClient() : base(_clientHandler) {}
}
