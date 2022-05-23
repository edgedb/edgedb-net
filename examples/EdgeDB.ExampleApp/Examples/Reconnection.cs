using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class Reconnection : IExample
    {
        public ILogger? Logger { get; set; }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            await using (var singleClinet = await client.GetOrCreateClientAsync<EdgeDBBinaryClient>().ConfigureAwait(false))
            {
                await singleClinet.ReconnectAsync();

                Logger!.LogInformation("Reconnected client is connected: {Connected}", singleClinet.IsConnected);
            }
        }
    }
}
