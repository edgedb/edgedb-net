using EdgeDB;
using EdgeDB.CIL;
using EdgeDB.ExampleApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Reflection.Metadata.Ecma335;
using static System.Net.Mime.MediaTypeNames;

unsafe
{
    Func<int, int> test = (i) =>
    {
        delegate*<int, int> ptr = &Test;

        var result = ptr(4);

        return i + 1;
    };

    static int Test(int a)
    {
        return a;
    }

    var reader = new ILReader(test.Method);

    List<Instruction> inst = new();

    while (reader.ReadNext(out var i))
        inst.Add(i);

    var ldftn = inst.FirstOrDefault(x => x.OpCodeType == OpCodeType.Calli);

    var token = MetadataTokens.EntityHandle((int)ldftn.Oprand!);

    var f = ldftn.OprandAsSignature();
}

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
