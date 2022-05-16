using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EdgeDB;
using EdgeDB.Tests.Benchmarks;

BenchmarkRunner.Run<Benchmarks>();

await Task.Delay(-1);

public class Benchmarks
{
    internal static MockedEdgeDBClient SingleClient;
    internal static EdgeDBClient ClientPool;

    static Benchmarks()
    {
        SingleClient = new MockedEdgeDBClient(0);
        ClientPool = new EdgeDBClient(new EdgeDBClientPoolConfig
        {
            ClientType = EdgeDBClientType.Custom,
            ClientFactory = (id) => ValueTask.FromResult<BaseEdgeDBClient>(new MockedEdgeDBClient(id)),
            DefaultPoolSize = 100
        });
    }


    // benchmark our default client as overhead
    [Benchmark]
    public static async Task BenchmarkQueryOverhead()
    {
        await SingleClient.QueryAsync<string>("select \"Hello, World!\"").ConfigureAwait(false);
    }

    // define our main benchmark
    [Benchmark]
    public static async Task BenchmarkQuery()
    {
        await ClientPool!.QueryAsync<string>("select \"Hello, World!\"").ConfigureAwait(false);
    }
}