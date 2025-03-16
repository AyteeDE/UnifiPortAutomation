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
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task Run()
    {
        var unifiConnection = await Connections.Unifi.UnifiAPIConnection.CreateAsync(Environment.GetEnvironmentVariable("UNIFI_USERNAME"), Environment.GetEnvironmentVariable("UNIFI_PASSWORD"), Environment.GetEnvironmentVariable("UNIFI_HOST"));
        await unifiConnection.DisposeAsync();
    }
}
