using Microsoft.Extensions.Logging;

namespace EdgeDB.ExampleApp.Examples;

internal class GlobalsAndConfig : IExample
{
    public ILogger? Logger { get; set; }

    public async Task ExecuteAsync(GelClientPool baseClientPool)
    {
        var clientPool = baseClientPool
            .WithConfig(conf => conf.AllowDMLInFunctions = true)
            .WithGlobals(new Dictionary<string, object?> {{"current_user_id", Guid.NewGuid()}});

        var result = await clientPool.QueryRequiredSingleAsync<Guid>("select global current_user_id");
        Logger!.LogInformation("CurrentUserId: {@Id}", result);
    }
}
