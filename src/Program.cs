using tibberservice;
using tibberservice.Infrastructure;
using tibberservice.Model;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddTransient<TibberClient>();
        services.Configure<TibberConfig>(context.Configuration.GetSection("Tibber"));
        services.Configure<InfluxConfig>(context.Configuration.GetSection("Influx"));

        services.AddHostedService<RealTimeService>();
        services.AddTransient<InfluxWriter>();
    })
    .Build();

await host.RunAsync();
