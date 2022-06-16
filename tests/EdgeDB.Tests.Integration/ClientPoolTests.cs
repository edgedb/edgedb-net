using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
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
        public async Task TestPoolQueryMethods()
        {
            var jsonResult = await _edgedb.QueryJsonAsync("select {(a := 1), (a := 2)}");
            Assert.Equal("[{\"a\" : 1}, {\"a\" : 2}]", jsonResult);

            var querySingleResult = await _edgedb.QuerySingleAsync<long>("select 123").ConfigureAwait(false);
            Assert.Equal(123, querySingleResult);
        }

        [Fact]
        public async Task TestPoolDisconnects()
        {
            await using var client = await _edgedb.GetOrCreateClientAsync();
            await client.DisconnectAsync(); // should be removed from the pool
            Assert.DoesNotContain(client, _edgedb.Clients);
            await client.DisposeAsync(); // should NOT be returned to the pool
            Assert.DoesNotContain(client, _edgedb.Clients);
        }

        [Fact]
        public async Task TestPoolRelease()
        {
            BaseEdgeDBClient client;
            await using (client = await _edgedb.GetOrCreateClientAsync())
            {
                await Task.Delay(100);
            }

            // client should be back in the pool
            Assert.Contains(client, _edgedb.Clients);
        }

        [Fact]
        public async Task TestPoolTransactions()
        {
            var result = await _edgedb.TransactionAsync(async (tx) =>
            {
                return await tx.QuerySingleAsync<string>("select \"Transaction within pools\"");
            });

            Assert.Equal("Transaction within pools", result);
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
