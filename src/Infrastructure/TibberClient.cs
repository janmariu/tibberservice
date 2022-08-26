using Microsoft.Extensions.Options;
using Tibber.Sdk;
using tibberservice.Model;

namespace tibberservice.Infrastructure;

public class TibberClient
{
    private readonly TibberConfig _config;

    public TibberClient(IOptions<TibberConfig> config)
    {
        config.Value.ThrowIfNotValid();
        _config = config.Value;
    }

    public async Task<IObservable<RealTimeMeasurement>> StartListener(Guid homeId, CancellationToken stopToken)
    {
        var apiClient = new TibberApiClient(_config.Key, _config.GetHeader());
        return await apiClient.StartRealTimeMeasurementListener(homeId, stopToken);
    }
    
    public async Task<IEnumerable<Home>> GetRealTimeConsumptionHomes()
    {
        var apiClient = new TibberApiClient(_config.Key, _config.GetHeader());
        var basicData = await apiClient.GetBasicData();
        return basicData
            .Data
            .Viewer
            .Homes
            .Where(home => home.Features.RealTimeConsumptionEnabled == true && home.Id != null);
    }
}