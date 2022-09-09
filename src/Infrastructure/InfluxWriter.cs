using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;
using tibberservice.Model;

namespace tibberservice.Infrastructure;

public class InfluxWriter
{
    private readonly ILogger<InfluxWriter> _logger;
    private readonly InfluxConfig _configuration;
    private readonly InfluxDBClient _client;
    
    public InfluxWriter(ILogger<InfluxWriter> logger, IOptions<InfluxConfig> configuration)
    {
        _logger = logger;
        _configuration = configuration.Value;
        _client = InfluxDBClientFactory.Create(
            _configuration.HttpHost,
            _configuration.ApiToken);
    }

    public async Task Write<T>(T measurement, string bucketName)
    {
        try
        {
            var writeApi = _client.GetWriteApiAsync();
            await writeApi.WriteMeasurementAsync(
                measurement,
                WritePrecision.S,
                bucketName,
                _configuration.Organization);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to write to influx");
        }
    }
}