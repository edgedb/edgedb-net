using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotTrace;
using EdgeDB.Tests.Benchmarks.Utils;

namespace EdgeDB.Tests.Benchmarks;

[DotTraceDiagnoser]
public class FullExecuteBenchmark
{
    public EdgeDBClient? Client;

    [GlobalSetup]
    public void Setup() =>
        Client = new EdgeDBClient(new EdgeDBClientPoolConfig
        {
            ClientFactory = (id, c, cng) =>
            {
                var client = new MockQueryClient(c, cng, null!, id);
                return ValueTask.FromResult<BaseEdgeDBClient>(client);
            }
        });

    [Benchmark]
    public Task<IReadOnlyCollection<string?>> FullExecuteAsync() =>
        Client!.QueryAsync<string>("select \"Hello, World!\"");
}
