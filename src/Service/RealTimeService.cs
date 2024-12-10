using tibberservice.Infrastructure;

namespace tibberservice;

public class RealTimeService : BackgroundService
{
    private readonly ILogger<RealTimeService> _logger;
    private readonly TibberClient _tibberClient;
    private readonly InfluxWriter _influxWriter;
    private readonly PostgresWriter _postgresWriter;

    public RealTimeService(
        ILogger<RealTimeService> logger, 
        TibberClient tibberClient,
        InfluxWriter influxWriter,
        PostgresWriter postgresWriter)
    {
        _logger = logger;
        _tibberClient = tibberClient;
        _influxWriter = influxWriter;
        _postgresWriter = postgresWriter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var homes = await _tibberClient.GetRealTimeConsumptionHomes();
            var observers = homes.Select(
                home => new HomeObserver(home, _tibberClient, _influxWriter, _postgresWriter)).ToList();

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var homeObserver in observers)
                {
                    await homeObserver.StartIfNeeded(stoppingToken);
                }
            
                await Task.Delay(30000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            Environment.Exit(1);
        }
    }
}