using System.Threading.Channels;
using tibberservice.Infrastructure;
using tibberservice.Model;

namespace tibberservice;

public class RealTimeService : BackgroundService
{
    private readonly ILogger<RealTimeService> _logger;
    private readonly TibberClient _tibberClient;
    private readonly InfluxWriter _influxWriter;
    private readonly PostgresWriter _postgresWriter;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly Channel<PowerMeasurement> _channel;
    private long _writtenCount;

    public RealTimeService(
        ILogger<RealTimeService> logger, 
        TibberClient tibberClient,
        InfluxWriter influxWriter,
        PostgresWriter postgresWriter,
        IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _tibberClient = tibberClient;
        _influxWriter = influxWriter;
        _postgresWriter = postgresWriter;
        _applicationLifetime = applicationLifetime;
        _channel = Channel.CreateUnbounded<PowerMeasurement>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var homes = await _tibberClient.GetRealTimeConsumptionHomes();

            foreach (var home in homes)
            {
                var sub = await _tibberClient.StartListener(home.Id!.Value, stoppingToken);
                sub.Subscribe(
                    onNext: value => {
                        var data = PowerMeasurement.Create(value, home.AppNickname);
                        if (!_channel.Writer.TryWrite(data))
                        {
                            _logger.LogWarning("Dropping measurements for {HomeId} at {Timestamp}", home.Id, value.Timestamp);
                        }
                    },
                    onError: ex => _logger.LogError(ex, "stream error"),
                    onCompleted: () => _logger.LogInformation("stream completed")
                );
            }            

            var processingTask = ProcessMeasurements(stoppingToken);
            var monitorTask = MonitorStats(stoppingToken);

            await Task.WhenAll(processingTask, monitorTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in ExecuteAsync");
            Environment.ExitCode = 1;
            throw;
        }
    }

    private async Task ProcessMeasurements(CancellationToken stoppingToken)
    {
        await foreach (var measurement in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _influxWriter.Write(measurement, "bitbucket");
                _postgresWriter.Write(measurement);
                Interlocked.Increment(ref _writtenCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist measurement for {Home}", measurement.HomeName);
                Environment.ExitCode = 1;
                _applicationLifetime.StopApplication();
            }
        }
    }

    private async Task MonitorStats(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var count = Interlocked.Exchange(ref _writtenCount, 0);
            _logger.LogInformation("Measurements written in the last minute: {Count}", count);
            if (count == 0)
            {
                _logger.LogInformation("No readings received for 1 minute. Exiting to let systemd restart.");
                Environment.ExitCode = 1;
                _applicationLifetime.StopApplication();
            }
        }
    }
}