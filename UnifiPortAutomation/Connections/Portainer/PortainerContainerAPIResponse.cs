using System;
using System.Text.Json.Serialization;

namespace UnifiPortAutomation.Connections.Portainer;

public class PortainerContainerAPIResponse
{
    public string Id { get; set; }
    public List<string> Names { get; set; }
    public int EnvironmentId { get; set; }
}
