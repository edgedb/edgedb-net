using EdgeDB.DataTypes;
using EdgeDB.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration
{
    [TestClass]
    public class ClientTests
    {
        internal EdgeDBClient EdgeDB { get; set; }
        private readonly Func<CancellationToken> _getToken;
        
        public ClientTests()
        {
            EdgeDB = ClientProvider.EdgeDB;
            _getToken = () => ClientProvider.GetTimeoutToken();
        }

        [TestMethod]
        public async Task TestCommandLocks()
        {
            await using var client = await EdgeDB.GetOrCreateClientAsync<EdgeDBBinaryClient>(token: _getToken());
            var timeoutToken = new CancellationTokenSource();
            timeoutToken.CancelAfter(1000);
            using var firstLock = await client.AquireCommandLockAsync(timeoutToken.Token);

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
            {
                var secondLock = await client.AquireCommandLockAsync(timeoutToken.Token);
            });
        }

        [TestMethod]
        public async Task TestPoolQueryMethods()
        {
            var jsonResult = await EdgeDB.QueryJsonAsync("select {(a := 1), (a := 2)}", token: _getToken());
            Assert.AreEqual("[{\"a\" : 1}, {\"a\" : 2}]", jsonResult.Value);

            var queryJsonElementsResult = await EdgeDB.QueryJsonElementsAsync("select {(a := 1), (a := 2)}", token: _getToken());

            Assert.AreEqual(2, queryJsonElementsResult.Count());

            Assert.AreEqual("{\"a\" : 1}", queryJsonElementsResult.First().Value);
            Assert.AreEqual("{\"a\" : 2}", queryJsonElementsResult.Last().Value);

            var querySingleResult = await EdgeDB.QuerySingleAsync<long>("select 123", token: _getToken()).ConfigureAwait(false);
            Assert.AreEqual(123, querySingleResult);

            var queryRequiredSingeResult = await EdgeDB.QueryRequiredSingleAsync<long>("select 123", token: _getToken());
            Assert.AreEqual(123, queryRequiredSingeResult);
        }

        [TestMethod]
        public async Task TestPoolRelease()
        {
            BaseEdgeDBClient client;
            await using (client = await EdgeDB.GetOrCreateClientAsync(token: _getToken()))
            {
                await Task.Delay(100);
            }

            // client should be back in the pool
            Assert.IsTrue(EdgeDB.Clients.Contains(client));
        }

        [TestMethod]
        public virtual async Task TestPoolTransactions()
        {
            var result = await EdgeDB.TransactionAsync(async (tx) =>
            {
                return await tx.QuerySingleAsync<string>("select \"Transaction within pools\"", token: _getToken());
            });

            Assert.AreEqual("Transaction within pools", result);
        }

        [TestMethod]
        public async Task StandardScalarQueries()
        {
            await TestScalarQuery("true", true);
            await TestScalarQuery("b'bina\\x01ry'", Encoding.UTF8.GetBytes("bina\x01ry"));
            await TestScalarQuery("<datetime>'1999-03-31T15:17:00Z'", DateTimeOffset.Parse("1999-03-31T15:17:00Z"));
            await TestScalarQuery("<cal::local_datetime>'1999-03-31T15:17:00'", System.DateTime.Parse("1999-03-31T15:17:00"));
            await TestScalarQuery<DateOnly>("<cal::local_date>'1999-03-31'", new(1999, 3, 31));
            await TestScalarQuery<TimeSpan>("<cal::local_time>'15:17:00'", new(15,17,0));
            await TestScalarQuery("42.0n", (decimal)42.0);
            await TestScalarQuery("3.14", 3.14f);
            await TestScalarQuery("314e-2", 314e-2);
            await TestScalarQuery<short>("<int16>1234", 1234);
            await TestScalarQuery("<int32>123456", 123456);
            await TestScalarQuery<long>("1234", 1234);
            await TestScalarQuery("\"Hello, Tests!\"", "Hello, Tests!");
        }

        [TestMethod]
        public async Task ArrayQueries()
        {
            await TestScalarQuery("[1,2,3]", new long[] { 1, 2, 3 });
            await TestScalarQuery("[\"Hello\", \"World\"]", new string[] { "Hello", "World" });
        }

        [TestMethod]
        public async Task TupleQueries()
        {
            var result = await EdgeDB.QuerySingleAsync<(long one, long two)>("select (1,2)", token: _getToken());
            Assert.AreEqual(1, result.one);
            Assert.AreEqual(2, result.two);

            var (one, two, three, four, five, six, seven, eight, nine, ten) = await EdgeDB.QuerySingleAsync<(long one, long two, long three, long four, long five, long six, long seven, long eight, long nine, long ten)>("select (1,2,3,4,5,6,7,8,9,10)", token: _getToken());
            Assert.AreEqual(1, one);
            Assert.AreEqual(2, two);
            Assert.AreEqual(3, three);
            Assert.AreEqual(4, four);
            Assert.AreEqual(5, five);
            Assert.AreEqual(6, six);
            Assert.AreEqual(7, seven);
            Assert.AreEqual(8, eight);
            Assert.AreEqual(9, nine);
            Assert.AreEqual(10, ten);

            var result2 = await EdgeDB.QuerySingleAsync<(long one, long two)>("select (one := 1, two := 2)", token: _getToken());
            Assert.AreEqual(1, result2.one);
            Assert.AreEqual(2, result2.two);
        }

        [TestMethod]
        public async Task SetQueries()
        {
            var result = await EdgeDB.QueryAsync<long>("select {1,2}", token: _getToken());
            Assert.AreEqual(1, result.First());
            Assert.AreEqual(2, result.Last());
        }

        [TestMethod]
        public async Task DisconnectAndReconnect()
        {
            // using raw client for this one,
            var client = await EdgeDB.GetOrCreateClientAsync(token: _getToken());

            // disconnect should close the underlying connection, and remove alloc'd resources for said connection.
            await client.DisconnectAsync(token: _getToken());

            // should run just fine, restarting the underlying connection.
            var str = await client.QueryRequiredSingleAsync<string>("select \"Hello, EdgeDB.Net!\"", token: _getToken());

            Assert.AreEqual("Hello, EdgeDB.Net!", str);
        }

        [TestMethod]
        public void StateChange()
        {
            var client2 = EdgeDB.WithConfig(x => x.DDLPolicy = DDLPolicy.AlwaysAllow);

            Assert.IsNull(EdgeDB.Config.DDLPolicy);
            Assert.AreEqual(DDLPolicy.AlwaysAllow, client2.Config.DDLPolicy);

            var client3 = client2.WithModule("test_module");

            Assert.IsNull(EdgeDB.Config.DDLPolicy);
            Assert.AreEqual(DDLPolicy.AlwaysAllow, client2.Config.DDLPolicy);
            Assert.AreEqual("test_module", client3.Module);
            Assert.AreNotEqual("test_module", client2.Module);
            Assert.AreNotEqual("test_module", EdgeDB.Module);
        }

        private async Task TestScalarQuery<TResult>(string select, TResult expected)
        {
            var result = await EdgeDB.QuerySingleAsync<TResult>($"select {select}", token: _getToken());
           
            switch(result)
            {
                case byte[] bt:
                    Assert.IsTrue(bt.ReflectionSequenceEqual((expected as byte[])!));
                    break;
                case long[] lg:
                    Assert.IsTrue(lg.ReflectionSequenceEqual((expected as long[])!));
                    break;
                case string[] st:
                    Assert.IsTrue(st.ReflectionSequenceEqual((expected as string[])!));
                    break;
                default:
                    Assert.AreEqual(expected, result);
                    break;
            }
        }
    }
}
