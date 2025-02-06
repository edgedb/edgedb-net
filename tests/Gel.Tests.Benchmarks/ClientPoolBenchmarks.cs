using BenchmarkDotNet.Attributes;

namespace Gel.Tests.Benchmarks;

public class ClientPoolBenchmarks
{
    internal static MockedGelClient SingleClient;
    internal static GelClientPool ClientPool;

    static ClientPoolBenchmarks()
    {
        SingleClient = new MockedGelClient(0);
        ClientPool = new GelClientPool(new GelClientPoolConfig
        {
            ClientType = GelClientType.Custom,
            ClientFactory = (id, _, _) => ValueTask.FromResult<BaseGelClient>(new MockedGelClient(id)),
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
