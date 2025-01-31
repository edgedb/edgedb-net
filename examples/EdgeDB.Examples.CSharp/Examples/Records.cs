using Microsoft.Extensions.Logging;

namespace EdgeDB.ExampleApp.Examples;

internal class Records : IExample
{
    public ILogger? Logger { get; set; }

    public async Task ExecuteAsync(GelClientPool clientPool)
    {
        var people = await clientPool.QueryAsync<Person>("select Person { name, email }");

        Logger!.LogInformation("People: {@People}", people);
    }

    public record Person(string Name, string Email);
}
