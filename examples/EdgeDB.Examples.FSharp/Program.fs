open ExampleRunner
open Serilog
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

Log.Logger <-
    LoggerConfiguration()
        .Enrich.FromLogContext()
        .MinimumLevel.Debug()
        .WriteTo.Console(outputTemplate = "{Timestamp:HH:mm:ss} - {Level}: {Message:lj}{NewLine}{Exception}")
        .CreateLogger()

let host =
    Host
        .CreateDefaultBuilder()
        .ConfigureServices(fun services ->
            services
                .AddLogging(fun logBuilder -> logBuilder.ClearProviders().AddSerilog(dispose = true) |> ignore)
                .AddEdgeDB(clientConfig = fun c -> c.SchemaNamingStrategy <- INamingStrategy.SnakeCaseNamingStrategy)
                .AddSingleton<ExampleRunner>()
            |> ignore)
        .Build()

host.Services.GetRequiredService<ExampleRunner>().RunAsync()
|> Async.AwaitTask
|> Async.RunSynchronously
