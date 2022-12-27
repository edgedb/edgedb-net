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
            using var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromTicks(5));

            try
            {
                await client.QueryRequiredSingleAsync<string>("select \"Hello, World\"", token: tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Logger!.LogInformation("Got task cancelled exception");
            }
        }
    }
}
