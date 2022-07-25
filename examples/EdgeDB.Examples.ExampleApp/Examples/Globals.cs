using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class Globals : IExample
    {
        public ILogger? Logger { get; set; }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            // get or create a client singleton with the state configuring delegate.
            // Note: the set state we define here is only for that instances default lifetime.
            // When the client is returned to the pool, the state is reset.
            await using var clientInstance = await client.GetOrCreateClientAsync(state =>
            {
                state.WithGlobals(new Dictionary<string, object?>
                {
                    {"current_user_id", Guid.NewGuid() }
                });
            });
            
            // select out the global
            var result = await clientInstance.QuerySingleAsync<Guid>("select global current_user_id");

            Logger?.LogInformation("Selected Global: {@Global}", result);
        }
    }
}
