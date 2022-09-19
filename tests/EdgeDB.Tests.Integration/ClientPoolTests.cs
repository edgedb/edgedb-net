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
    }
}
