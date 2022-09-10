using Tibber.Sdk;
using tibberservice.Infrastructure;
using tibberservice.Model;

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
            _listener = await _tibberClient.StartListener(this._home.Id!.Value, stopToken);
            _listener.Subscribe(this);
            _running = true;
        }
    }
}