using EdgeDB.DataTypes;
using Microsoft.Extensions.Logging;

namespace EdgeDB.ExampleApp.Examples;

internal class RangeExample : IExample
{
    public ILogger? Logger { get; set; }

    public async Task ExecuteAsync(EdgeDBClient client)
    {
        var range = await client.QuerySingleAsync<Range<long>>("select range(1, 10)");
    }
}
