using tibberservice.Infrastructure;
using System.Threading.Channels;
using tibberservice.Model;

namespace tibberservice;

public class RealTimeService : BackgroundService
{
    private readonly ILogger<RealTimeService> _logger;
    private readonly TibberClient _tibberClient;
    private readonly ChannelWriter<PowerMeasurement> _channelWriter;

    public RealTimeService(
        ILogger<RealTimeService> logger, 
        TibberClient tibberClient,
        ChannelWriter<PowerMeasurement> channelWriter)
    {
        _logger = logger;
        _tibberClient = tibberClient;
        _channelWriter = channelWriter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var homes = await _tibberClient.GetRealTimeConsumptionHomes();
            var observers = homes.Select(
                home => new HomeObserver(home, _tibberClient, _channelWriter)).ToList();

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