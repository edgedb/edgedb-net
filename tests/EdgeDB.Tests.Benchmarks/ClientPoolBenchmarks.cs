using BenchmarkDotNet.Attributes;

namespace EdgeDB.Tests.Benchmarks;

public class ClientPoolBenchmarks
{
    internal static MockedEdgeDBClient SingleClient;
    internal static GelClientPool ClientPool;

    static ClientPoolBenchmarks()
    {
        SingleClient = new MockedEdgeDBClient(0);
        ClientPool = new GelClientPool(new GelClientPoolConfig
        {
            ClientType = EdgeDBClientType.Custom,
            ClientFactory = (id, _, _) => ValueTask.FromResult<BaseEdgeDBClient>(new MockedEdgeDBClient(id)),
            DefaultPoolSize = 100
        });
    }

    // benchmark our default client as overhead
    [Benchmark]
    public async Task BenchmarkQueryOverhead() =>
        await SingleClient.QueryAsync<string>("select \"Hello, World!\"").ConfigureAwait(false);

    // define our main benchmark
    [Benchmark]
    public async Task BenchmarkQuery() =>
        await ClientPool!.QueryAsync<string>("select \"Hello, World!\"").ConfigureAwait(false);
}
