using System.Runtime.InteropServices;
using tibberservice;
using tibberservice.Infrastructure;
using tibberservice.Model;

var builder = Host.CreateDefaultBuilder(args);

var runningOnSystemd = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

if (runningOnSystemd)
{
    builder.UseSystemd();
}
else
{
    builder.ConfigureLogging(logging =>
    {
        logging.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            options.SingleLine = true;
        });
    });
}

builder.ConfigureServices((context, services) =>
{
    services.AddTransient<TibberClient>();
    services.Configure<PostgresConfig>(context.Configuration.GetSection("Postgres"));
    services.Configure<TibberConfig>(context.Configuration.GetSection("Tibber"));
    services.Configure<InfluxConfig>(context.Configuration.GetSection("Influx"));
    
    services.AddTransient<InfluxWriter>();
    services.AddTransient<PostgresWriter>();
    services.AddHostedService<RealTimeService>();
});

await builder.Build().RunAsync();
