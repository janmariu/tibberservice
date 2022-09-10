using System.Runtime.InteropServices;
using tibberservice;
using tibberservice.Infrastructure;
using tibberservice.Model;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddTransient<TibberClient>();
        services.Configure<TibberConfig>(context.Configuration.GetSection("Tibber"));
        services.Configure<InfluxConfig>(context.Configuration.GetSection("Influx"));

        services.AddHostedService<RealTimeService>();
        services.AddTransient<InfluxWriter>();
    });

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    builder.UseSystemd();
}

await builder.Build().RunAsync();
