using EdgeDB;
using EdgeDB.ExampleApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} - {Level}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

using var host = Host.CreateDefaultBuilder()
    .ConfigureServices((services) =>
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        services.AddEdgeDB(clientConfig: clientConfig =>
        {
            clientConfig.SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy;
            clientConfig.ClientType = EdgeDBClientType.Tcp;
        });

        services.AddSingleton<ExampleRunner>();
    }).Build();

await host.Services.GetRequiredService<ExampleRunner>().StartAsync();

// hault the program
await Task.Delay(-1);
