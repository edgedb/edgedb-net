using EdgeDB;
using EdgeDB.CIL;
using EdgeDB.ExampleApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Linq.Expressions;

new TestClass().Test();
    
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
        });

        services.AddSingleton<ExampleRunner>();
    }).Build();

await host.Services.GetRequiredService<ExampleRunner>().StartAsync();

// hault the program
await Task.Delay(-1);

public class TestClass
{
    public void Test()
    {
        Func<int, int> fun = (i) => i + 1;

        Expression<Func<uint>> t = () => 1;

        var exp = CILInterpreter.InterpretFunc(fun);

        var translated = ExpressionTranslator.Translate(exp);
    }
}
