using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class CancelQueries : IExample
    {
        public ILogger? Logger { get; set; }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            await using var singleClient = await client.GetOrCreateClientAsync<EdgeDBBinaryClient>();

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromTicks(5));

            try
            {
                await singleClient.QueryRequiredSingleAsync<string>("select \"Hello, World\"", token: tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Logger!.LogInformation("Got task cancelled exception, client is connected? {Conn}", singleClient.IsConnected);
            }
        }
    }
}
