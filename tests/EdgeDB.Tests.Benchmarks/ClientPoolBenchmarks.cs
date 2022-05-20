using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Benchmarks
{
    public class ClientPoolBenchmarks
    {
        internal static MockedEdgeDBClient SingleClient;
        internal static EdgeDBClient ClientPool;

        static ClientPoolBenchmarks()
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
        public async Task BenchmarkQueryOverhead()
        {
            await SingleClient.QueryAsync<string>("select \"Hello, World!\"").ConfigureAwait(false);
        }

        // define our main benchmark
        [Benchmark]
        public async Task BenchmarkQuery()
        {
            await ClientPool!.QueryAsync<string>("select \"Hello, World!\"").ConfigureAwait(false);
        }
    }
}
