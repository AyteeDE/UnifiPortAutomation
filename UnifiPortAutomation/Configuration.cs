using System;

namespace UnifiPortAutomation;

public class Configuration
{
    public int EnvironmentId { get; set; }
    public string Name { get; set; }
    public string Id { get; set; }
    public List<int> Ports { get; set; }
}
