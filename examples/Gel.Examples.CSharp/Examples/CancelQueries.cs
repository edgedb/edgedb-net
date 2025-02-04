using Microsoft.Extensions.Logging;

namespace Gel.ExampleApp.Examples;

internal class CancelQueries : IExample
{
    public ILogger? Logger { get; set; }

    public async Task ExecuteAsync(GelClientPool clientPool)
    {
        using var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(TimeSpan.FromTicks(5));

        try
        {
            await clientPool.QueryRequiredSingleAsync<string>("select \"Hello, World\"", token: tokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            Logger!.LogInformation("Got task cancelled exception");
        }
    }
}
