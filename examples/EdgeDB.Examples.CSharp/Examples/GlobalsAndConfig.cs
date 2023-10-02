using Microsoft.Extensions.Logging;

namespace EdgeDB.ExampleApp.Examples;

internal class GlobalsAndConfig : IExample
{
    public ILogger? Logger { get; set; }

    public async Task ExecuteAsync(EdgeDBClient baseClient)
    {
        var client = baseClient
            .WithConfig(conf => conf.AllowDMLInFunctions = true)
            .WithGlobals(new Dictionary<string, object?> {{"current_user_id", Guid.NewGuid()}});

        var result = await client.QueryRequiredSingleAsync<Guid>("select global current_user_id");
        Logger!.LogInformation("CurrentUserId: {@Id}", result);
    }
}
