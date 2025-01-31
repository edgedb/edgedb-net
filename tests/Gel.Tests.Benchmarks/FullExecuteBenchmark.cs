using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotTrace;
using Gel.Tests.Benchmarks.Utils;

namespace Gel.Tests.Benchmarks;

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
                return ValueTask.FromResult<BaseGelClient>(client);
            }
        });

    [Benchmark]
    public Task<IReadOnlyCollection<string?>> FullExecuteAsync() =>
        ClientPool!.QueryAsync<string>("select \"Hello, World!\"");
}
