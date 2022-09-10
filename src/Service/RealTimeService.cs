using Tibber.Sdk;
using tibberservice.Infrastructure;
using tibberservice.Model;

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
        var observers = homes.Select(home => new HomeObserver(home, _tibberClient, _influxWriter)).ToList();

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
            
            await Task.Delay(5000, stoppingToken);
        }
    }
}

public class HomeObserver : IObserver<RealTimeMeasurement>
{
    private readonly Home _home;
    private readonly TibberClient _tibberClient;
    private readonly InfluxWriter _influxWriter;
    private bool _running = false;
    public HomeObserver(
        Home home,
        TibberClient tibberClient,
        InfluxWriter influxWriter)
    {
        _home = home;
        _tibberClient = tibberClient;
        _influxWriter = influxWriter;
    }
    
    public void OnCompleted()
    {
        _running = false;
    }

    public void OnError(Exception error)
    {
        _running = false;
    }

    public void OnNext(RealTimeMeasurement value)
    {
        var m = PowerMeasurement.Create(value, _home.AppNickname);
        _influxWriter.Write(m, "bitbucket").Wait();
        Console.WriteLine($"{value.Timestamp}: {value.Power}");
    }


    private IObservable<RealTimeMeasurement>? _listener = null;

    public async Task StartIfNeeded(CancellationToken stopToken)
    {
        if (!_running)
        {
            _listener = await _tibberClient.StartListener(this._home.Id.Value, stopToken);
            _listener.Subscribe(this);
            _running = true;
        }
    }
}