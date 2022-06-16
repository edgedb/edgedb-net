using EdgeDB.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EdgeDB.Tests.Integration
{
    [CollectionDefinition("ClientTest", DisableParallelization = true)]
    public class ClientTests : IClassFixture<ClientFixture>
    {
        private readonly EdgeDBClient _edgedb;
        private readonly ITestOutputHelper _output;

        public ClientTests(ClientFixture clientFixture, ITestOutputHelper output)
        {
            _edgedb = clientFixture.EdgeDB;
            _output = output;
        }

        [Fact]
        public async Task StandardScalarQueries()
        {
            await TestScalarQuery("true", true);
            await TestScalarQuery("b'bina\\x01ry'", Encoding.UTF8.GetBytes("bina\x01ry"));
            await TestScalarQuery("<datetime>'1999-03-31T15:17:00Z'", DateTimeOffset.Parse("1999-03-31T15:17:00Z"));
            await TestScalarQuery("<cal::local_datetime>'1999-03-31T15:17:00'", DateTime.Parse("1999-03-31T15:17:00"));
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

        [Fact]
        public async Task ArrayQueries()
        {
            await TestScalarQuery("[1,2,3]", new long[] { 1, 2, 3 });
            await TestScalarQuery("[\"Hello\", \"World\"]", new string[] { "Hello", "World" });
        }

        [Fact]
        public async Task TupleQueries()
        {
            var result = await _edgedb.QuerySingleAsync<(long one, long two)>("select (1,2)");
            Assert.Equal(1, result.one);
            Assert.Equal(2, result.two);

            var (one, two, three, four, five, six, seven, eight, nine, ten) = await _edgedb.QuerySingleAsync<(long one, long two, long three, long four, long five, long six, long seven, long eight, long nine, long ten)>("select (1,2,3,4,5,6,7,8,9,10)");
            Assert.Equal(1, one);
            Assert.Equal(2, two);
            Assert.Equal(3, three);
            Assert.Equal(4, four);
            Assert.Equal(5, five);
            Assert.Equal(6, six);
            Assert.Equal(7, seven);
            Assert.Equal(8, eight);
            Assert.Equal(9, nine);
            Assert.Equal(10, ten);
        }

        [Fact]
        public async Task SetQueries()
        {
            var result = await _edgedb.QueryAsync<long>("select {1,2}");
            Assert.Equal(1, result.First());
            Assert.Equal(2, result.Last());
        }

        private async Task TestScalarQuery<TResult>(string select, TResult expected)
        {
            var result = await _edgedb.QuerySingleAsync<TResult>($"select {select}");
           
            switch(result)
            {
                case byte[] bt:
                    Assert.True(bt.SequenceEqual((expected as byte[])!));
                    break;

                default:
                    Assert.Equal(expected, result);
                    break;
            }
        }
    }
}
