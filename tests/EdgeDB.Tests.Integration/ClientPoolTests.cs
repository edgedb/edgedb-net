using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EdgeDB.Tests.Integration
{
    [CollectionDefinition("ClientPoolTest", DisableParallelization = true)]
    public class ClientPoolTests : IClassFixture<ClientFixture>
    {
        private readonly EdgeDBClient _edgedb;
        private readonly ITestOutputHelper _output;

        public ClientPoolTests(ClientFixture clientFixture, ITestOutputHelper output)
        {
            _edgedb = clientFixture.EdgeDB;
            _output = output;
        }

        [Fact]
        public async Task TestPoolCapability()
        {
            // create 1000 tasks
            var numTasks = 1000;
            Task[] tasks = new Task[numTasks];
            ConcurrentBag<string> results = new();

            for (int i = 0; i != numTasks; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    results.Add(await _edgedb.QueryRequiredSingleAsync<string>("select \"Hello, Dotnet!\""));
                });
            }

            _output.WriteLine("Starting 1000 query test...");

            Stopwatch sw = Stopwatch.StartNew();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            sw.Stop();

            Assert.Equal(1000, results.Count);
            Assert.All(results, x => Assert.Equal("Hello, Dotnet!", x));

            _output.WriteLine($"Executed 1000 query test in {sw.ElapsedMilliseconds}ms");
        }
    }
}
