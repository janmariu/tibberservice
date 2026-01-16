using System.Runtime.InteropServices;
using System.Threading.Channels;
using tibberservice;
using tibberservice.Infrastructure;
using tibberservice.Model;
using tibberservice.Service;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddTransient<TibberClient>();
        services.Configure<PostgresConfig>(context.Configuration.GetSection("Postgres"));
        services.Configure<TibberConfig>(context.Configuration.GetSection("Tibber"));
        services.Configure<InfluxConfig>(context.Configuration.GetSection("Influx"));
        
        var measurementChannel = Channel.CreateUnbounded<PowerMeasurement>();
        services.AddSingleton(measurementChannel);
        services.AddSingleton(measurementChannel.Writer);
        services.AddSingleton(measurementChannel.Reader);
        
        services.AddTransient<InfluxWriter>();
        services.AddTransient<PostgresWriter>();
        services.AddHostedService<RealTimeService>();
        services.AddHostedService<MeasurementWriteService>();
    });

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    builder.UseSystemd();
}

await builder.Build().RunAsync();
