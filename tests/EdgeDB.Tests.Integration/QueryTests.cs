using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration
{
    [TestClass]
    public class QueryTests
    {
        private readonly EdgeDBClient _client;

        public QueryTests()
        {
            _client = ClientProvider.EdgeDB;
        }

        private async Task TestTypeQuerying<TType>(string tname, TType expected, Func<TType, TType, bool>? comparer = null)
        {
            var actual = await _client.QueryRequiredSingleAsync<TType>($"select <{tname}>$value", new Dictionary<string, object?>
            {
                {"value", expected }
            });

            if(comparer is not null)
            {
                Assert.IsTrue(comparer(expected, actual));
            }
            else if (expected is IEnumerable a && actual is IEnumerable b)
            {
                Assert.IsTrue(a.ReflectionSequenceEqual(b));
            }
            else if (expected is DataTypes.Json j1 && actual is DataTypes.Json j2)
            {
                Assert.AreEqual(j1.Value, j2.Value);
            }
            else
            {
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public async Task TestPlainObjectDeserialization()
        {
            var result = await _client.QueryRequiredSingleAsync<dynamic>("select { a := 1, b := 'hello', c := { d := 2, e := 'world'} }");

            Assert.IsInstanceOfType(result.a, typeof(long));
            Assert.IsInstanceOfType(result.b, typeof(string));
            Assert.IsInstanceOfType(result.c.d, typeof(long));
            Assert.IsInstanceOfType(result.c.e, typeof(string));

            Assert.AreEqual(1, result.a);
            Assert.AreEqual("hello", result.b);
            Assert.AreEqual(2, result.c.d);
            Assert.AreEqual("world", result.c.e);
        }

        [TestMethod]
        public async Task TupleQueries()
        {
            var result = await _client.QueryRequiredSingleAsync<(long one, long two)>("select (1,2)");
            Assert.AreEqual(1, result.one);
            Assert.AreEqual(2, result.two);

            var (one, two, three, four, five, six, seven, eight, nine, ten) = await _client.QueryRequiredSingleAsync<(long one, long two, long three, long four, long five, long six, long seven, long eight, long nine, long ten)>("select (1,2,3,4,5,6,7,8,9,10)");
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

            var result2 = await _client.QueryRequiredSingleAsync<(long one, long two)>("select (one := 1, two := 2)");
            Assert.AreEqual(1, result2.one);
            Assert.AreEqual(2, result2.two);
        }

        [TestMethod]
        public async Task SetQueries()
        {
            var result = await _client.QueryAsync<long>("select {1,2}");
            Assert.AreEqual(1, result.First());
            Assert.AreEqual(2, result.Last());
        }

        #region Arrays
        private async Task TestArrayQuerying<T>(string tname, T[] expected)
            => await TestTypeQuerying($"array<{tname}>", expected);
        private async Task TestArrayOfRange<T>(string tname, DataTypes.Range<T>[] expected)
            where T : struct
            => await TestArrayQuerying($"range<{tname}>", expected);

        [TestMethod]
        public async Task TestArrayOfScalars()
        {
            await TestArrayQuerying("int16", new short[] { 1, 2, 5, 9, 24 });
            await TestArrayQuerying("int32", new int[] { 1, 2, 5, 9, 24 });
            await TestArrayQuerying("int64", new long[] { 1, 2, 5, 9, 24 });

            await TestArrayQuerying("str", new string[] { "Hello", "EdgeDB", "Dot", "Net" });

            await TestArrayQuerying("bool", new bool[] { true, false, false, true });

            await TestArrayQuerying("float32", new float[] { 1.1f, 2.2f, 3.141592654f, 5.5f });
            await TestArrayQuerying("float64", new double[] { 1.1, 2.2, 3.141592654, 5.5 });

            await TestArrayQuerying("decimal", new decimal[] { 1.1M, 2.2M, 3.141592654M, 5.5M });

            await TestArrayQuerying("bigint", new BigInteger[] { (BigInteger)long.MaxValue, 12444, 245156 });

            await TestArrayQuerying("datetime", new DataTypes.DateTime[] { DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-3) });
            await TestArrayQuerying("cal::local_datetime", new DataTypes.LocalDateTime[] { DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-3) });

            // sys datetime types
            await TestArrayQuerying("datetime", new DateTime[]
            {
                DateTime.Now.AddDays(-1).RoundToMicroseconds(),
                DateTime.Now.AddDays(-2).RoundToMicroseconds(),
                DateTime.Now.AddDays(-3).RoundToMicroseconds()
            });
            
            await TestArrayQuerying("cal::local_datetime", new DateTime[]
            {
                DateTime.Now.AddDays(-1).RoundToMicroseconds(),
                DateTime.Now.AddDays(-2).RoundToMicroseconds(),
                DateTime.Now.AddDays(-3).RoundToMicroseconds()
            });

        }

        [TestMethod]
        public async Task TestArrayOfRangeScalars()
        {
            await TestArrayOfRange("int32", new DataTypes.Range<int>[]
            {
                new DataTypes.Range<int>(1, 2),
                new DataTypes.Range<int>(3, 7),
                new DataTypes.Range<int>(8, 44),
            });

            await TestArrayOfRange("int64", new DataTypes.Range<long>[]
            {
                new DataTypes.Range<long>(1, 2),
                new DataTypes.Range<long>(3, 7),
                new DataTypes.Range<long>(8, 44),
            });

            await TestArrayOfRange("float32", new DataTypes.Range<float>[]
            {
                new DataTypes.Range<float>(1.4f, 2.45f),
                new DataTypes.Range<float>(3.141f, 7.6832f),
                new DataTypes.Range<float>(8.92f, 44.224f),
            });

            await TestArrayOfRange("float64", new DataTypes.Range<double>[]
            {
                new DataTypes.Range<double>(1.4d, 2.45d),
                new DataTypes.Range<double>(3.141d, 7.6832d),
                new DataTypes.Range<double>(8.92d, 44.224d),
            });

            await TestArrayOfRange("datetime", new DataTypes.Range<DataTypes.DateTime>[]
            {
                new DataTypes.Range<DataTypes.DateTime>(DateTime.Now.AddDays(-5), DateTime.Now),
                new DataTypes.Range<DataTypes.DateTime>(DateTime.Now.AddYears(-1), DateTime.Now),
                new DataTypes.Range<DataTypes.DateTime>(DateTime.Now.AddMinutes(-2), DateTime.Now.AddYears(4)),
            });

            // sys datetime
            await TestArrayOfRange("datetime", new DataTypes.Range<DateTime>[]
            {
                new DataTypes.Range<DateTime>(DateTime.Now.AddDays(-5).RoundToMicroseconds(), DateTime.Now.RoundToMicroseconds()),
                new DataTypes.Range<DateTime>(DateTime.Now.AddYears(-1).RoundToMicroseconds(), DateTime.Now.RoundToMicroseconds()),
                new DataTypes.Range<DateTime>(DateTime.Now.AddMinutes(-2).RoundToMicroseconds(), DateTime.Now.AddYears(4).RoundToMicroseconds()),
            });
        }
        #endregion

        #region Ranges
        private async Task TestRangeQuerying<T>(string tname, DataTypes.Range<T> expected)
            where T : struct
            => await TestTypeQuerying($"range<{tname}>", expected);

        [TestMethod]
        public Task TestRangeOfInt32()
            => TestRangeQuerying("int32", new DataTypes.Range<int>(1, 40));

        [TestMethod]
        public Task TestRangeOfInt64()
            => TestRangeQuerying("int64", new DataTypes.Range<long>(1, 40));

        [TestMethod]
        public Task TestRangeOfFloat32()
            => TestRangeQuerying("float32", new DataTypes.Range<float>(1.5f, 400.5f));

        [TestMethod]
        public Task TestRangeOfFloat64()
            => TestRangeQuerying("float64", new DataTypes.Range<double>(1.5, 400.5));

        [TestMethod]
        public Task TestRangeOfDecimal()
            => TestRangeQuerying("decimal", new DataTypes.Range<decimal>(1.5M, 400.5M));

        [TestMethod]
        public Task TestRangeOfDateTime()
            => TestRangeQuerying("datetime", new DataTypes.Range<DataTypes.DateTime>(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow));

        [TestMethod]
        public Task TestRangeOfSystemDateTime()
            => TestRangeQuerying("datetime", new DataTypes.Range<DateTime>(DateTime.Now.AddDays(-2).RoundToMicroseconds(), DateTime.Now.RoundToMicroseconds()));

        [TestMethod]
        public Task TestRangeOfSystemDateTimeOffset()
            => TestRangeQuerying("datetime", new DataTypes.Range<DateTimeOffset>(DateTime.UtcNow.AddDays(-2).RoundToMicroseconds(), DateTime.UtcNow.RoundToMicroseconds()));

        [TestMethod]
        public Task TestRangeOfLocalDateTime()
            => TestRangeQuerying("cal::local_datetime", new DataTypes.Range<DataTypes.LocalDateTime>(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow));

        [TestMethod]
        public Task TestRangeOfSystemLocalDateTime()
            => TestRangeQuerying("cal::local_datetime", new DataTypes.Range<DateTime>(DateTime.Now.AddDays(-2).RoundToMicroseconds(), DateTime.Now.RoundToMicroseconds()));

        [TestMethod]
        public Task TestRangeOfSystemLocalDateTimeOffset()
            => TestRangeQuerying("cal::local_datetime", new DataTypes.Range<DateTimeOffset>(DateTime.UtcNow.AddDays(-2).RoundToMicroseconds(), DateTime.UtcNow.RoundToMicroseconds()));

        [TestMethod]
        public Task TestRangeOfLocalDate()
            => TestRangeQuerying("cal::local_date", new DataTypes.Range<DataTypes.LocalDate>(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow)));

        [TestMethod]
        public Task TestRangeOfSystemLocalDate()
            => TestRangeQuerying("cal::local_date", new DataTypes.Range<DateOnly>(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow)));
        #endregion

        #region Scalars
        [TestMethod]
        public Task TestUUID()
            => TestTypeQuerying("uuid", Guid.NewGuid());

        [TestMethod]
        public Task TestStr()
            => TestTypeQuerying("str", "test");

        [TestMethod]
        public Task TestBytes()
            => TestTypeQuerying("bytes", new byte[] { 1, 2, 3, 4 });

        [TestMethod]
        public Task TestInt16()
            => TestTypeQuerying("int16", (short)1337);

        [TestMethod]
        public Task TestInt32()
            => TestTypeQuerying("int32", Random.Shared.Next());

        [TestMethod]
        public Task TestInt64()
            => TestTypeQuerying("int64", Random.Shared.NextInt64());

        [TestMethod]
        public Task TestFloat32()
            => TestTypeQuerying("float32", Random.Shared.NextSingle());

        [TestMethod]
        public Task TestFloat64()
            => TestTypeQuerying("float64", Random.Shared.NextDouble());

        [TestMethod]
        public Task TestDecimal()
            => TestTypeQuerying("decimal", -15000.6250000M);

        [TestMethod]
        public Task TestBool()
            => TestTypeQuerying("bool", Random.Shared.Next(1) == 1);

        [TestMethod]
        public Task TestDateTime()
            => TestTypeQuerying<DataTypes.DateTime>("datetime", RandomDateTime());

        [TestMethod]
        public Task TestDuration()
            => TestTypeQuerying<DataTypes.Duration>("duration", RandomTimeSpan());

        [TestMethod]
        public Task TestJson()
            => TestTypeQuerying<DataTypes.Json>("json", new("{\"name\": \"test\"}"));

        [TestMethod]
        public Task TestLocalDateTime()
            => TestTypeQuerying<DataTypes.LocalDateTime>("cal::local_datetime", RandomDTO());

        [TestMethod]
        public Task TestLocalDate()
            => TestTypeQuerying<DataTypes.LocalDate>("cal::local_date", RandomDate());

        [TestMethod]
        public Task TestLocalTime()
            => TestTypeQuerying<DataTypes.LocalTime>("cal::local_time", TimeOnly.FromDateTime(DateTime.UtcNow));

        [TestMethod]
        public Task TestBigInt()
            => TestTypeQuerying<BigInteger>("bigint", -15000);

        [TestMethod]
        public Task TestRelativeDuration()
            => TestTypeQuerying<DataTypes.RelativeDuration>("cal::relative_duration", RandomTimeSpan());

        [TestMethod]
        public Task TestDateDuration()
            => TestTypeQuerying<DataTypes.DateDuration>("cal::date_duration", TimeSpan.FromDays(Random.Shared.Next(999)));

        [TestMethod]
        public Task TestMemory()
            => TestTypeQuerying<DataTypes.Memory>("cfg::memory", new(Random.Shared.NextInt64()));

        [TestMethod]
        public Task TestDateTimeLegacy()
            => TestTypeQuerying("cal::local_datetime", RandomDateTime());

        [TestMethod]
        public Task TestDateTimeOffsetLegacy()
            => TestTypeQuerying("datetime", RandomDTO());

        [TestMethod]
        public Task TestTimeOnlyLegacy()
            => TestTypeQuerying("cal::local_time", TimeOnly.FromDateTime(DateTime.UtcNow), (e, a) => CompareMicroseconds(e.ToTimeSpan(), a.ToTimeSpan()));

        [TestMethod]
        public Task TestDateOnlyLegacy()
            => TestTypeQuerying("cal::local_date", RandomDate());
        #endregion

        #region Typeless queries

        private Task TestTypelessQuery<T>(string toSelect, T expected)
            => TestTypelessQuery<T>(_client, toSelect, expected);

        private async Task TestTypelessQuery<T>(EdgeDBClient client, string toSelect, T expected)
        {
            var result = await client.QueryRequiredSingleAsync<object>($"select {toSelect}");

            Assert.IsInstanceOfType(result, typeof(T));
            Assert.AreEqual(expected, result);
        }

        private Task TestTypelessQuery<T>(string toSelect, Predicate<T> predicate)
            => TestTypelessQuery<T>(_client, toSelect, predicate);
        private async Task TestTypelessQuery<T>(EdgeDBClient client, string toSelect, Predicate<T> predicate)
        {
            var result = await client.QueryRequiredSingleAsync<object>($"select {toSelect}");

            Assert.IsInstanceOfType(result, typeof(T));
            Assert.IsTrue(predicate((T)result));
        }

        [TestMethod]
        public async Task TestTypelessFreeObjectQuery()
        {
            var result = await _client.QueryRequiredSingleAsync<dynamic>("select { a := 1, b := \"Foo\", c := { ca := \"Bar\", cb := <uuid>\"4a0e4b46-b6b1-11ed-95ac-b35bb41e8bbc\" }, d := {1,2,3} }");

            Assert.AreEqual(1L, result.a);
            Assert.AreEqual("Foo", result.b);
            Assert.IsNotNull(result.c);
            Assert.AreEqual("Bar", result.c.ca);
            Assert.AreEqual(Guid.Parse("4a0e4b46-b6b1-11ed-95ac-b35bb41e8bbc"), result.c.cb);
            Assert.AreEqual(3, result.d.Length);
            Assert.IsTrue(Enumerable.SequenceEqual(result.d, new long[] {1,2,3}));
        }

        [TestMethod]
        public async Task TestTypelesBasicQueries()
        {
            await TestTypelessQuery("1", 1L);
            await TestTypelessQuery("'FooBar'", "FooBar");
        }

        [TestMethod]
        public async Task TestTypelessTemporalQueries()
        {
            // model types
            var client = ClientProvider.ConfigureClient(c => c.PreferSystemTemporalTypes = false);

            await TestTypelessQuery<DataTypes.DateTime>(
                client, "<datetime>'2018-05-07T15:01:22+00'",
                new DataTypes.DateTime(DateTime.Parse("2018-05-07T15:01:22+00"))
            );

            await TestTypelessQuery<DataTypes.Duration>(
                client, "<duration>'48 hours 45 minutes'",
                new DataTypes.Duration(new TimeSpan(48, 45, 0))
            );

            await TestTypelessQuery<DataTypes.LocalDateTime>(
                client, "<cal::local_datetime>'2018-05-07T15:01:22.306916'",
                new DataTypes.LocalDateTime(DateTime.Parse("2018-05-07T15:01:22.306916").ToLocalTime())
            );

            await TestTypelessQuery<DataTypes.LocalDate>(
                client, "<cal::local_date>'2018-05-07'",
                new DataTypes.LocalDate(DateOnly.Parse("2018-05-07"))
            );

            await TestTypelessQuery<DataTypes.LocalTime>(
                client, "<cal::local_time>'15:01:22.306916'",
                new DataTypes.LocalTime(TimeOnly.Parse("15:01:22.306916"))
            );

            await TestTypelessQuery<DataTypes.RelativeDuration>(
                client, "<cal::relative_duration>'1 year'",
                new DataTypes.RelativeDuration(months: 12)
            );

            await TestTypelessQuery<DataTypes.DateDuration>(
                 client, "<cal::date_duration>'5 days'",
                 new DataTypes.DateDuration(TimeSpan.FromDays(5))
            );

            // system types
            client = ClientProvider.ConfigureClient(c => c.PreferSystemTemporalTypes = true);

            await TestTypelessQuery<DateTimeOffset>(
                client, "<datetime>'2018-05-07T15:01:22+00'",
                DateTimeOffset.Parse("2018-05-07T15:01:22+00")
            );

            await TestTypelessQuery<TimeSpan>(
                client, "<duration>'48 hours 45 minutes'",
                new TimeSpan(48, 45, 0)
            );

            await TestTypelessQuery<DateTime>(
                client, "<cal::local_datetime>'2018-05-07T15:01:22.306916'",
                DateTime.Parse("2018-05-07T15:01:22.306916").ToLocalTime()
            );

            await TestTypelessQuery<DateOnly>(
                client, "<cal::local_date>'2018-05-07'",
                DateOnly.Parse("2018-05-07")
            );

            await TestTypelessQuery<TimeOnly>(
                client, "<cal::local_time>'15:01:22.306916'",
                TimeOnly.Parse("15:01:22.306916")
            );

            await TestTypelessQuery<TimeSpan>(
                client, "<cal::relative_duration>'1 day'",
                TimeSpan.FromDays(1)
            );

            await TestTypelessQuery<TimeSpan>(
                 client, "<cal::date_duration>'5 days'",
                 TimeSpan.FromDays(5)
            );
        }

        [TestMethod]
        public async Task TestTypelessArrayQuery()
        {
            await TestTypelessQuery<long[]>("[1,2,3]", v => v.SequenceEqual(new long[] { 1, 2, 3 }));
            await TestTypelessQuery<string[]>("['Foo', 'Bar', 'Baz']", v => v.SequenceEqual(new string[] { "Foo", "Bar", "Baz" }));
        }

        [TestMethod]
        public async Task TestTypelessSetOfArrayAndRange()
        {
            var longResult = (await _client.QueryAsync<long[]>("select {[1,2], [3,4]}")).ToArray();

            Assert.AreEqual(2, longResult.Length);
            Assert.IsTrue(longResult[0]!.SequenceEqual(new long[] { 1, 2 }));
            Assert.IsTrue(longResult[1]!.SequenceEqual(new long[] { 3, 4 }));

            var strResult = (await _client.QueryAsync<string[]>("select {['Foo'], ['Bar', 'Baz']}")).ToArray();

            Assert.AreEqual(2, strResult.Length);
            Assert.IsTrue(strResult[0]!.SequenceEqual(new string[] { "Foo" }));
            Assert.IsTrue(strResult[1]!.SequenceEqual(new string[] { "Bar", "Baz" }));
        }

        #endregion

        #region Helpers
        private static bool CompareMicroseconds(TimeSpan a, TimeSpan b)
        {
            // since the system type ('a' in this case) can be in tick-presision, round
            // as we would for standard temporals.
            return Math.Round(a.TotalMicroseconds) == b.TotalMicroseconds;
        }

        private TimeSpan RandomTimeSpan()
        {
            var ticks = Random.Shared.NextInt64(TimeSpan.Zero.Ticks, TimeSpan.FromDays(100).Ticks);
            return TimeSpan.FromTicks((ticks / 1000) * 1000); // convert to microsecond precision to match db
        }

        private DateTime RandomDateTime()
        {
            var secs = Random.Shared.NextInt64((long)(DateTime.Now - DateTime.MinValue).TotalMilliseconds);
            return DateTime.MinValue.AddMilliseconds(secs).RoundToMicroseconds(); // convert to microsecond precision to match db
        }

        private DateTimeOffset RandomDTO()
        {
            return RandomDateTime().ToUniversalTime().RoundToMicroseconds();
        }

        private DateOnly RandomDate()
        {
            return DateOnly.FromDateTime(RandomDateTime());
        }
        #endregion
    }
}
