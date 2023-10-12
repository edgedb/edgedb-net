using Microsoft.Extensions.Logging;

namespace EdgeDB.ExampleApp.Examples;

public class Transactions : IExample
{
    public ILogger? Logger { get; set; }

    public async Task ExecuteAsync(EdgeDBClient client)
    {
        // we can enter a transaction by calling TransactionAsync on a client.
        var str = await client.TransactionAsync(async tx =>
        {
            // inside our transaction we can preform queries using the 'tx' object.
            // ontop of this, anything we return in the transaction is returned by the
            // TransactionAsync function.
            return await tx.QueryRequiredSingleAsync<string>("select \"Hello World!\"");
        });

        Logger?.LogInformation("TransactionResult: {Result}", str);
    }
}
