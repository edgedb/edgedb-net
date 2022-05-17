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
            var result = await client.RestoreDatabaseAsync(dumpStream!).ConfigureAwait(false);

            // Log the status of the restore
            Logger?.LogInformation("Restore status: {Status}", result.Status);
        }
    }
}
