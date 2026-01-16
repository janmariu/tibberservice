using System.Threading.Channels;
using tibberservice.Infrastructure;
using tibberservice.Model;

namespace tibberservice.Service;

public class MeasurementWriteService : BackgroundService
{
    private readonly ChannelReader<PowerMeasurement> _reader;
    private readonly InfluxWriter _influxWriter;
    private readonly PostgresWriter _postgresWriter;
    private readonly ILogger<MeasurementWriteService> _logger;

    public MeasurementWriteService(
        ChannelReader<PowerMeasurement> reader,
        InfluxWriter influxWriter,
        PostgresWriter postgresWriter,
        ILogger<MeasurementWriteService> logger)
    {
        _reader = reader;
        _influxWriter = influxWriter;
        _postgresWriter = postgresWriter;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var measurement in _reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _influxWriter.Write(measurement, "bitbucket");
                _postgresWriter.Write(measurement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist measurement");
            }
        }
    }
}
