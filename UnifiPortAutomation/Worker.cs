using System.ComponentModel;
using System.Text.Json;
using UnifiPortAutomation.Connections.Portainer;
using UnifiPortAutomation.Connections.Unifi;

namespace UnifiPortAutomation;

public class Worker : BackgroundService
{
    private static int _interval;

    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        int intervalMinutes = 60;
        int.TryParse(Environment.GetEnvironmentVariable("INTERVAL_MINUTES"), out intervalMinutes);

        _interval = intervalMinutes * 60 * 1000;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Run();
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Next run at: {time}", DateTimeOffset.Now.AddMilliseconds(_interval));
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task Run()
    {
        var containerConfigurationJson = Environment.GetEnvironmentVariable("PORTAINER_CONTAINER_CONFIGURATION");
        List<Configuration> containerConfigurations = JsonSerializer.Deserialize<List<Configuration>>(containerConfigurationJson);
        var usedEnvironments = GetUsedEnvironmentsForPortainer(containerConfigurations);

        var unifiConnection = await Connections.Unifi.UnifiAPIConnection.CreateAsync(Environment.GetEnvironmentVariable("UNIFI_USERNAME"), Environment.GetEnvironmentVariable("UNIFI_PASSWORD"), Environment.GetEnvironmentVariable("UNIFI_HOST"));
        var fwds = await unifiConnection.GetPortForwardingsAsync();

        var portainerConnection = await Connections.Portainer.PortainerAPIConnection.CreateAsync(Environment.GetEnvironmentVariable("PORTAINER_HOST"), Environment.GetEnvironmentVariable("PORTAINER_TOKEN"));
        List<PortainerContainerAPIResponse> containers = new List<PortainerContainerAPIResponse>();
        foreach(var environmentId in usedEnvironments)
        {
            var containersForEnvironment = await portainerConnection.GetRunningContainers(environmentId);
            foreach(var container in containersForEnvironment)
            {
                container.EnvironmentId = environmentId;
            }
            containers.AddRange(containersForEnvironment);
        }

        foreach(var fwd in fwds.Data)
        {
            if(fwd.Enabled != CheckIfPortIsNeeded(fwd, containerConfigurations, containers))
            {
                //if current port status is different from needed port status, update to Unifi API
                fwd.Enabled = !fwd.Enabled;
                await unifiConnection.PutPortForwardingAsync(fwd);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation($"Port Forwarding for Port {fwd.Dst_Port} set to {(fwd.Enabled ? "enabled" : "disabled")}");
                }
            }
        }
        await unifiConnection.DisposeAsync(); //Dispose connection to logout
    }
    private List<int> GetUsedEnvironmentsForPortainer(List<Configuration> containerConfigurations)
    {
        var grouped = containerConfigurations.GroupBy(c => c.EnvironmentId).Select(g => g.Key).ToList();
        return grouped;
    }
    private bool CheckIfPortIsNeeded(UnifiAPIResponseData fwd, List<Configuration> configurations, List<PortainerContainerAPIResponse> containers)
    {
        int port;
        if(!int.TryParse(fwd.Fwd_Port, out port)) //port is not an int, return current status
        {
            return fwd.Enabled;
        }

        var matchingConfig = configurations.FirstOrDefault(c => c.Ports.Any(p => p == port));
        if(matchingConfig == null) //port not in config, return current status
        {
            return fwd.Enabled;
        }

        var machingContainer = containers.FirstOrDefault(c => c.EnvironmentId == matchingConfig.EnvironmentId && (c.Names.Contains($"/{matchingConfig.Name}") || c.Id == matchingConfig.Id));
        if(machingContainer == null) //no running container
        {
            return false;
        }

        //Container with matching port is running -> return true
        return true;
    }
}
