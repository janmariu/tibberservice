using Microsoft.Extensions.Options;
using Tibber.Sdk;
using tibberservice.Model;

namespace tibberservice.Infrastructure;

public class TibberClient
{
    private readonly TibberConfig _config;
    private readonly TibberApiClient _apiClient;

    public TibberClient(IOptions<TibberConfig> config)
    {
        config.Value.ThrowIfNotValid();
        _config = config.Value;
        _apiClient = new TibberApiClient(_config.Key, _config.GetHeader());
    }

    public async Task<IObservable<RealTimeMeasurement>> StartListener(Guid homeId, CancellationToken stopToken) =>
        await _apiClient.StartRealTimeMeasurementListener(homeId, stopToken);

    public async Task StopListener(Guid homeId) => await _apiClient.StopRealTimeMeasurementListener(homeId);
    
    public async Task<IEnumerable<Home>> GetRealTimeConsumptionHomes()
    {
        var basicData = await _apiClient.GetBasicData();
        return basicData
            .Data
            .Viewer
            .Homes
            .Where(home => home.Features.RealTimeConsumptionEnabled == true && home.Id != null);
    }
}