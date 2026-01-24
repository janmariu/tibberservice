using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
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
        _client = new InfluxDBClient(_configuration.HttpHost, _configuration.ApiToken);
    }

    public void Write<T>(T measurement, string bucketName)
    {
        try
        {
            var writeApi = _client.GetWriteApi();
            writeApi.WriteMeasurement(
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