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
            // reconnect a single client
            await using (var singleClinet = await client.GetOrCreateClientAsync<EdgeDBBinaryClient>().ConfigureAwait(false))
            {
                await singleClinet.ReconnectAsync();

                Logger!.LogInformation("Reconnected client is connected: {Connected}", singleClinet.IsConnected);
            }

            Logger!.LogInformation("Current clients before disconnect: {Clients}", client.ConnectedClients);
            
            // disconnect all clients
            await client.DisconnectAllAsync();

            Logger!.LogInformation("Current clients after disconnect: {Clients}", client.ConnectedClients);
        }
    }
}
