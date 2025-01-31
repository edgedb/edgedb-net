using Gel.DataTypes;
using Microsoft.Extensions.Logging;

namespace Gel.ExampleApp.Examples;

internal class RangeExample : IExample
{
    public ILogger? Logger { get; set; }

    public async Task ExecuteAsync(GelClientPool clientPool)
    {
        var range = await clientPool.QuerySingleAsync<Range<long>>("select range(1, 10)");
    }
}
