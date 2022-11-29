using BenchmarkDotNet.Attributes;
using EdgeDB.Tests.Benchmarks.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Benchmarks
{
    public class FullExecuteBenchmark
    {
        public EdgeDBClient? Client;

        [GlobalSetup]
        public void Setup()
        {
            Client = new EdgeDBClient(new EdgeDBClientPoolConfig
            {
                ClientFactory = (id, c, cng) =>
                {
                    var client = new MockQueryClient(c, cng, null!, id);
                    return ValueTask.FromResult<BaseEdgeDBClient>(client);
                }
            });
        }

        [Benchmark]
        public Task<IReadOnlyCollection<string?>> FullExecuteAsync()
        {
            return Client!.QueryAsync<string>("select \"Hello, World!\"");
        }
    }
}
