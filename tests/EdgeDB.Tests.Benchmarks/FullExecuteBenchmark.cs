using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotTrace;
using EdgeDB.Tests.Benchmarks.Utils;

namespace EdgeDB.Tests.Benchmarks;

[DotTraceDiagnoser]
public class FullExecuteBenchmark
{
    public GelClientPool? ClientPool;

    [GlobalSetup]
    public void Setup() =>
        ClientPool = new GelClientPool(new GelClientPoolConfig
        {
            ClientFactory = (id, c, cng) =>
            {
                var client = new MockQueryClient(c, cng, null!, id);
                return ValueTask.FromResult<BaseEdgeDBClient>(client);
            }
        });

    [Benchmark]
    public Task<IReadOnlyCollection<string?>> FullExecuteAsync() =>
        ClientPool!.QueryAsync<string>("select \"Hello, World!\"");
}
