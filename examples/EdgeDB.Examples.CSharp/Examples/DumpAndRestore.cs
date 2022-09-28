using Microsoft.Extensions.Logging;

namespace EdgeDB.ExampleApp.Examples
{
    public class DumpAndRestore : IExample
    {
        public ILogger? Logger { get; set; }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            // dump the database to a stream;
            using var dumpStream = await client.DumpDatabaseAsync().ConfigureAwait(false);

            // at this point you can write the stream to a file as a backup,
            // for this example we're just going to immediatly restore it.

            // since our database isn't empty we cannot restore it; but lets say it was empty, we could use the following code:
            // var result = await client.RestoreDatabaseAsync(dumpStream!).ConfigureAwait(false);
            // Logger?.LogInformation("Restore status: {Status}", result.Status);
        }
    }
}
