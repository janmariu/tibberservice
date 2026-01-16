using Tibber.Sdk;
using tibberservice.Infrastructure;
using tibberservice.Model;
using System.Threading.Channels;

public class HomeObserver : IObserver<RealTimeMeasurement>
{
    private readonly Home _home;
    private readonly TibberClient _tibberClient;
    private readonly ChannelWriter<PowerMeasurement> _channelWriter;
    private bool _running;
    
    public HomeObserver(
        Home home,
        TibberClient tibberClient,
        ChannelWriter<PowerMeasurement> channelWriter)
    {
        _home = home;
        _tibberClient = tibberClient;
        _channelWriter = channelWriter;
    }
    
    public void OnCompleted()
    {
        _running = false;
        _subscription?.Dispose();
    }

    public void OnError(Exception error)
    {
        _running = false;
        _subscription?.Dispose();
    }

    public void OnNext(RealTimeMeasurement value)
    {
        var m = PowerMeasurement.Create(value, _home.AppNickname);
        // Background writer service will pick up queued measurements.
        _channelWriter.TryWrite(m);
        Console.WriteLine($"{value.Timestamp}: {value.Power}");
    }

    private IObservable<RealTimeMeasurement>? _listener = null;
    private IDisposable? _subscription = null;
    
    public async Task StartIfNeeded(CancellationToken stopToken)
    {
        if (_running)
        {
            return;
        }

        _listener = await _tibberClient.StartListener(_home.Id!.Value, stopToken);
        _subscription = _listener.Subscribe(this);
        _running = true;
    }
}