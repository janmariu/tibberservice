using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;
using tibberservice.Model;

namespace tibberservice.Infrastructure;

public class InfluxWriter
{
    private readonly InfluxConfig _configuration;
    private readonly InfluxDBClient _client;
    
    public InfluxWriter(IOptions<InfluxConfig> configuration)
    {
        _configuration = configuration.Value;
        _client = InfluxDBClientFactory.Create(
            _configuration.HttpHost,
            _configuration.ApiToken);
    }

    public Task Write<T>(T measurement, string bucketName)
    {
        var writeApi = _client.GetWriteApiAsync();
        return writeApi.WriteMeasurementAsync(
            measurement,
            WritePrecision.S,
            bucketName,
            _configuration.Organization);
    }

    public Task Write<T>(List<T> measurements, string bucketName)
    {
        var writeApi = _client.GetWriteApiAsync();
        return writeApi.WriteMeasurementsAsync(measurements,
            WritePrecision.S,
            bucketName,
            _configuration.Organization);
    }
}