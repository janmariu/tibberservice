using tibberservice.Infrastructure;

namespace tibberservice;

public class RealTimeService : BackgroundService
{
    private readonly ILogger<RealTimeService> _logger;
    private readonly TibberClient _tibberClient;
    private readonly InfluxWriter _influxWriter;

    public RealTimeService(
        ILogger<RealTimeService> logger, 
        TibberClient tibberClient,
        InfluxWriter influxWriter)
    {
        _logger = logger;
        _tibberClient = tibberClient;
        _influxWriter = influxWriter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var homes = await _tibberClient.GetRealTimeConsumptionHomes();
        var observers = homes.Select(
            home => new HomeObserver(home, _tibberClient, _influxWriter)).ToList();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var homeObserver in observers)
                {
                    await homeObserver.StartIfNeeded(stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Threw exception when trying to start home observers.");
            }
            
            await Task.Delay(30000, stoppingToken);
        }
    }
}