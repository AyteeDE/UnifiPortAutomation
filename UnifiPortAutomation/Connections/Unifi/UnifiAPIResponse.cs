using System;
using System.Text.Json.Serialization;

namespace UnifiPortAutomation.Connections.Unifi;

public class UnifiAPIResponse
{
    [JsonPropertyName("data")]
    public List<UnifiAPIResponseData> Data { get; set; }
}
public class UnifiAPIResponseData
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("fwd_port")]
    public string Fwd_Port { get; set; }
    [JsonPropertyName("fwd")]
    public string Fwd { get; set; }
    [JsonPropertyName("dst_port")]
    public string Dst_Port { get; set; }
    [JsonPropertyName("destination_ip")]
    public string Destination_IP { get; set; }
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    public override string ToString()
    {
        return Name;
    }
}
