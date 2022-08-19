using System.Net.Http.Headers;

namespace tibberservice.Model;

public class TibberConfig
{
    public string UserAgent { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;
}

public static class TibberConfigExtensions
{
    public static void ThrowIfNotValid(this TibberConfig config)
    {
        if (config == null ||
            string.IsNullOrEmpty(config.Key) ||
            string.IsNullOrEmpty(config.UserAgent))
        {
            throw new ArgumentException(@"Missing tibber config. 
                Add tibber: { key: <yourkey>, UserAgent: <some useragent> } 
                to appsettings.json ");
        }
    }

    public static bool IsValid(this TibberConfig config)
    {
        return !string.IsNullOrEmpty(config.Key) && !string.IsNullOrEmpty(config.UserAgent);
    }

    public static ProductInfoHeaderValue GetHeader(this TibberConfig config)
    {
        return new ProductInfoHeaderValue(config.UserAgent, "1.0");
    }
}