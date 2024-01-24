using EdgeDB.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using DateTime = System.DateTime;

namespace EdgeDB.Tests.Integration;

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
        var actual = await _client.QueryRequiredSingleAsync<TType>($"select <{tname}>$value",
            new Dictionary<string, object?> {{"value", expected}});

        if (comparer is not null)
        {
            Assert.IsTrue(comparer(expected, actual));
        }
        else if (expected is IEnumerable a && actual is IEnumerable b)
        {
            Assert.IsTrue(a.ReflectionSequenceEqual(b));
        }
        else if (expected is Json j1 && actual is Json j2)
        {
            Assert.AreEqual(j1.Value, j2.Value);
        }
        else
        {
            Assert.AreEqual(expected, actual);
        }
    }

    [TestMethod]
    public async Task TestCollectionResutls()
    {
        await TestTypeQuerying("array<int64>", new long[] {1, 2, 3}, Enumerable.SequenceEqual);
        await TestTypeQuerying("array<int64>", (IEnumerable<long>)new long[] {1, 2, 3}, Enumerable.SequenceEqual);

        var result = await _client.QueryAsync<long>("select {1,2,3}");

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.SequenceEqual(new List<long> {1, 2, 3}));
    }

    [TestMethod]
    public async Task TestPlainObjectDeserialization()
    {
        var result =
            await _client.QueryRequiredSingleAsync<dynamic>(
                "select { a := 1, b := 'hello', c := { d := 2, e := 'world'} }");

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

        var (one, two, three, four, five, six, seven, eight, nine, ten) =
            await _client
                .QueryRequiredSingleAsync<(long one, long two, long three, long four, long five, long six, long seven,
                    long eight, long nine, long ten)>("select (1,2,3,4,5,6,7,8,9,10)");
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

    private async Task TestArrayOfRange<T>(string tname, Range<T>[] expected)
        where T : struct
        => await TestArrayQuerying($"range<{tname}>", expected);

    [TestMethod]
    public async Task TestArrayOfScalars()
    {
        await TestArrayQuerying("int16", new short[] {1, 2, 5, 9, 24});
        await TestArrayQuerying("int32", new[] {1, 2, 5, 9, 24});
        await TestArrayQuerying("int64", new long[] {1, 2, 5, 9, 24});

        await TestArrayQuerying("str", new[] {"Hello", "EdgeDB", "Dot", "Net"});

        await TestArrayQuerying("bool", new[] {true, false, false, true});

        await TestArrayQuerying("float32", new[] {1.1f, 2.2f, 3.141592654f, 5.5f});
        await TestArrayQuerying("float64", new[] {1.1, 2.2, 3.141592654, 5.5});

        await TestArrayQuerying("decimal", new[] {1.1M, 2.2M, 3.141592654M, 5.5M});

        await TestArrayQuerying("bigint", new BigInteger[] {long.MaxValue, 12444, 245156});

        await TestArrayQuerying("datetime",
            new DataTypes.DateTime[] {DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-3)});
        await TestArrayQuerying("cal::local_datetime",
            new LocalDateTime[] {DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-3)});

        // sys datetime types
        await TestArrayQuerying("datetime",
            new[]
            {
                DateTime.Now.AddDays(-1).RoundToMicroseconds(), DateTime.Now.AddDays(-2).RoundToMicroseconds(),
                DateTime.Now.AddDays(-3).RoundToMicroseconds()
            });

        await TestArrayQuerying("cal::local_datetime",
            new[]
            {
                DateTime.Now.AddDays(-1).RoundToMicroseconds(), DateTime.Now.AddDays(-2).RoundToMicroseconds(),
                DateTime.Now.AddDays(-3).RoundToMicroseconds()
            });
    }

    [TestMethod]
    public async Task TestArrayOfRangeScalars()
    {
        await TestArrayOfRange("int32", new Range<int>[] {new(1, 2), new(3, 7), new(8, 44)});

        await TestArrayOfRange("int64", new Range<long>[] {new(1, 2), new(3, 7), new(8, 44)});

        await TestArrayOfRange("float32",
            new Range<float>[] {new(1.4f, 2.45f), new(3.141f, 7.6832f), new(8.92f, 44.224f)});

        await TestArrayOfRange("float64",
            new Range<double>[] {new(1.4d, 2.45d), new(3.141d, 7.6832d), new(8.92d, 44.224d)});

        await TestArrayOfRange("datetime",
            new Range<DataTypes.DateTime>[]
            {
                new(DateTime.Now.AddDays(-5), DateTime.Now), new(DateTime.Now.AddYears(-1), DateTime.Now),
                new(DateTime.Now.AddMinutes(-2), DateTime.Now.AddYears(4))
            });

        // sys datetime
        await TestArrayOfRange("datetime",
            new Range<DateTime>[]
            {
                new(DateTime.Now.AddDays(-5).RoundToMicroseconds(), DateTime.Now.RoundToMicroseconds()),
                new(DateTime.Now.AddYears(-1).RoundToMicroseconds(), DateTime.Now.RoundToMicroseconds()),
                new(DateTime.Now.AddMinutes(-2).RoundToMicroseconds(),
                    DateTime.Now.AddYears(4).RoundToMicroseconds())
            });
    }

    #endregion

    #region Ranges

    private async Task TestRangeQuerying<T>(string tname, Range<T> expected)
        where T : struct
        => await TestTypeQuerying($"range<{tname}>", expected);

    [TestMethod]
    public Task TestRangeOfInt32()
        => TestRangeQuerying("int32", new Range<int>(1, 40));

    [TestMethod]
    public Task TestRangeOfInt64()
        => TestRangeQuerying("int64", new Range<long>(1, 40));

    [TestMethod]
    public Task TestRangeOfFloat32()
        => TestRangeQuerying("float32", new Range<float>(1.5f, 400.5f));

    [TestMethod]
    public Task TestRangeOfFloat64()
        => TestRangeQuerying("float64", new Range<double>(1.5, 400.5));

    [TestMethod]
    public Task TestRangeOfDecimal()
        => TestRangeQuerying("decimal", new Range<decimal>(1.5M, 400.5M));

    [TestMethod]
    public Task TestRangeOfDateTime()
        => TestRangeQuerying("datetime", new Range<DataTypes.DateTime>(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow));

    [TestMethod]
    public Task TestRangeOfSystemDateTime()
        => TestRangeQuerying("datetime",
            new Range<DateTime>(DateTime.Now.AddDays(-2).RoundToMicroseconds(), DateTime.Now.RoundToMicroseconds()));

    [TestMethod]
    public Task TestRangeOfSystemDateTimeOffset()
        => TestRangeQuerying("datetime",
            new Range<DateTimeOffset>(DateTime.UtcNow.AddDays(-2).RoundToMicroseconds(),
                DateTime.UtcNow.RoundToMicroseconds()));

    [TestMethod]
    public Task TestRangeOfLocalDateTime()
        => TestRangeQuerying("cal::local_datetime",
            new Range<LocalDateTime>(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow));

    [TestMethod]
    public Task TestRangeOfSystemLocalDateTime()
        => TestRangeQuerying("cal::local_datetime",
            new Range<DateTime>(DateTime.Now.AddDays(-2).RoundToMicroseconds(), DateTime.Now.RoundToMicroseconds()));

    [TestMethod]
    public Task TestRangeOfSystemLocalDateTimeOffset()
        => TestRangeQuerying("cal::local_datetime",
            new Range<DateTimeOffset>(DateTime.UtcNow.AddDays(-2).RoundToMicroseconds(),
                DateTime.UtcNow.RoundToMicroseconds()));

    [TestMethod]
    public Task TestRangeOfLocalDate()
        => TestRangeQuerying("cal::local_date",
            new Range<LocalDate>(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                DateOnly.FromDateTime(DateTime.UtcNow)));

    [TestMethod]
    public Task TestRangeOfSystemLocalDate()
        => TestRangeQuerying("cal::local_date",
            new Range<DateOnly>(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                DateOnly.FromDateTime(DateTime.UtcNow)));

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
        => TestTypeQuerying("bytes", new byte[] {1, 2, 3, 4});

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
        => TestTypeQuerying<Duration>("duration", RandomTimeSpan());

    [TestMethod]
    public Task TestJson()
        => TestTypeQuerying("json", new Json("{\"name\": \"test\"}"));

    [TestMethod]
    public Task TestLocalDateTime()
        => TestTypeQuerying<LocalDateTime>("cal::local_datetime", RandomDTO());

    [TestMethod]
    public Task TestLocalDate()
        => TestTypeQuerying<LocalDate>("cal::local_date", RandomDate());

    [TestMethod]
    public Task TestLocalTime()
        => TestTypeQuerying<LocalTime>("cal::local_time", TimeOnly.FromDateTime(DateTime.UtcNow));

    [TestMethod]
    public Task TestBigInt()
        => TestTypeQuerying<BigInteger>("bigint", -15000);

    [TestMethod]
    public Task TestRelativeDuration()
        => TestTypeQuerying<RelativeDuration>("cal::relative_duration", RandomTimeSpan());

    [TestMethod]
    public Task TestDateDuration()
        => TestTypeQuerying<DateDuration>("cal::date_duration", TimeSpan.FromDays(Random.Shared.Next(999)));

    [TestMethod]
    public Task TestMemory()
        => TestTypeQuerying("cfg::memory", new Memory(Random.Shared.NextInt64()));

    [TestMethod]
    public Task TestDateTimeLegacy()
        => TestTypeQuerying("cal::local_datetime", RandomDateTime());

    [TestMethod]
    public Task TestDateTimeOffsetLegacy()
        => TestTypeQuerying("datetime", RandomDTO());

    [TestMethod]
    public Task TestTimeOnlyLegacy()
        => TestTypeQuerying("cal::local_time", TimeOnly.FromDateTime(DateTime.UtcNow),
            (e, a) => CompareMicroseconds(e.ToTimeSpan(), a.ToTimeSpan()));

    [TestMethod]
    public Task TestDateOnlyLegacy()
        => TestTypeQuerying("cal::local_date", RandomDate());

    #endregion

    #region Typeless queries

    private Task TestTypelessQuery<T>(string toSelect, T expected)
        => TestTypelessQuery(_client, toSelect, expected);

    private async Task TestTypelessQuery<T>(EdgeDBClient client, string toSelect, T expected)
    {
        var result = await client.QueryRequiredSingleAsync<object>($"select {toSelect}");

        Assert.IsInstanceOfType(result, typeof(T));
        Assert.AreEqual(expected, result);
    }

    private Task TestTypelessQuery<T>(string toSelect, Predicate<T> predicate)
        => TestTypelessQuery(_client, toSelect, predicate);

    private async Task TestTypelessQuery<T>(EdgeDBClient client, string toSelect, Predicate<T> predicate)
    {
        var result = await client.QueryRequiredSingleAsync<object>($"select {toSelect}");

        Assert.IsInstanceOfType(result, typeof(T));
        Assert.IsTrue(predicate((T)result));
    }

    [TestMethod]
    public async Task TestTypelessFreeObjectQuery()
    {
        var result = await _client.QueryRequiredSingleAsync<dynamic>(
            "select { a := 1, b := \"Foo\", c := { ca := \"Bar\", cb := <uuid>\"4a0e4b46-b6b1-11ed-95ac-b35bb41e8bbc\" }, d := {1,2,3} }");

        Assert.AreEqual(1L, result.a);
        Assert.AreEqual("Foo", result.b);
        Assert.IsNotNull(result.c);
        Assert.AreEqual("Bar", result.c.ca);
        Assert.AreEqual(Guid.Parse("4a0e4b46-b6b1-11ed-95ac-b35bb41e8bbc"), result.c.cb);
        Assert.AreEqual(3, result.d.Length);
        Assert.IsTrue(Enumerable.SequenceEqual(result.d, new long[] {1, 2, 3}));
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

        await TestTypelessQuery(
            client, "<datetime>'2018-05-07T15:01:22+00'",
            new DataTypes.DateTime(DateTime.Parse("2018-05-07T15:01:22+00"))
        );

        await TestTypelessQuery(
            client, "<duration>'48 hours 45 minutes'",
            new Duration(new TimeSpan(48, 45, 0))
        );

        await TestTypelessQuery(
            client, "<cal::local_datetime>'2018-05-07T15:01:22.306916'",
            new LocalDateTime(DateTime.Parse("2018-05-07T15:01:22.306916").ToLocalTime())
        );

        await TestTypelessQuery(
            client, "<cal::local_date>'2018-05-07'",
            new LocalDate(DateOnly.Parse("2018-05-07"))
        );

        await TestTypelessQuery(
            client, "<cal::local_time>'15:01:22.306916'",
            new LocalTime(TimeOnly.Parse("15:01:22.306916"))
        );

        await TestTypelessQuery(
            client, "<cal::relative_duration>'1 year'",
            new RelativeDuration(months: 12)
        );

        await TestTypelessQuery(
            client, "<cal::date_duration>'5 days'",
            new DateDuration(TimeSpan.FromDays(5))
        );

        // system types
        client = ClientProvider.ConfigureClient(c => c.PreferSystemTemporalTypes = true);

        await TestTypelessQuery(
            client, "<datetime>'2018-05-07T15:01:22+00'",
            DateTimeOffset.Parse("2018-05-07T15:01:22+00")
        );

        await TestTypelessQuery(
            client, "<duration>'48 hours 45 minutes'",
            new TimeSpan(48, 45, 0)
        );

        await TestTypelessQuery(
            client, "<cal::local_datetime>'2018-05-07T15:01:22.306916'",
            DateTime.Parse("2018-05-07T15:01:22.306916").ToLocalTime()
        );

        await TestTypelessQuery(
            client, "<cal::local_date>'2018-05-07'",
            DateOnly.Parse("2018-05-07")
        );

        await TestTypelessQuery(
            client, "<cal::local_time>'15:01:22.306916'",
            TimeOnly.Parse("15:01:22.306916")
        );

        await TestTypelessQuery(
            client, "<cal::relative_duration>'1 day'",
            TimeSpan.FromDays(1)
        );

        await TestTypelessQuery(
            client, "<cal::date_duration>'5 days'",
            TimeSpan.FromDays(5)
        );
    }

    [TestMethod]
    public async Task TestTypelessArrayQuery()
    {
        await TestTypelessQuery<long[]>("[1,2,3]", v => v.SequenceEqual(new long[] {1, 2, 3}));
        await TestTypelessQuery<string[]>("['Foo', 'Bar', 'Baz']", v => v.SequenceEqual(new[] {"Foo", "Bar", "Baz"}));
    }

    [TestMethod]
    public async Task TestTypelessSetOfArrayAndRange()
    {
        var longResult = (await _client.QueryAsync<long[]>("select {[1,2], [3,4]}")).ToArray();

        Assert.AreEqual(2, longResult.Length);
        Assert.IsTrue(longResult[0]!.SequenceEqual(new long[] {1, 2}));
        Assert.IsTrue(longResult[1]!.SequenceEqual(new long[] {3, 4}));

        var strResult = (await _client.QueryAsync<string[]>("select {['Foo'], ['Bar', 'Baz']}")).ToArray();

        Assert.AreEqual(2, strResult.Length);
        Assert.IsTrue(strResult[0]!.SequenceEqual(new[] {"Foo"}));
        Assert.IsTrue(strResult[1]!.SequenceEqual(new[] {"Bar", "Baz"}));
    }

    [TestMethod]
    public async Task TestTypelessRange() =>
        await TestTypelessQuery<Range<long>>("range(1,10)", v => v.Lower == 1 && v.Upper == 10);

    #endregion

    #region Object types

    public class ScalarContainer
    {
        public short? A { get; set; }
        public int? B { get; set; }
        public long? C { get; set; }
        public string? D { get; set; }
        public bool? E { get; set; }
        public float? F { get; set; }
        public double? G { get; set; }
        public BigInteger? H { get; set; }
        public decimal? I { get; set; }
        public Guid? J { get; set; }
        public Json? K { get; set; }
        public EdgeDB.DataTypes.DateTime? L { get; set; }
        public DataTypes.LocalDateTime? M { get; set; }
        public DataTypes.LocalDate? N { get; set; }
        public DataTypes.LocalTime? O { get; set; }
        public DataTypes.Duration? P { get; set; }
        public DataTypes.RelativeDuration? Q { get; set; }
        public DataTypes.DateDuration? R { get; set; }
        public byte[]? S { get; set; }

    }

    [TestMethod]
    public async Task TestNullableScalars()
    {
        var j = Guid.NewGuid();
        var k = new Json("{\"Test\": \"Value\"}");
        var l = new DataTypes.DateTime(DateTime.Now);
        var m = new LocalDateTime(DateTime.Now);
        var n = new LocalDate(DateOnly.FromDateTime(DateTime.Now));
        var o = new LocalTime(TimeOnly.FromDateTime(DateTime.Now));
        var p = new Duration(TimeSpan.FromSeconds(23));
        var q = new RelativeDuration(TimeSpan.FromMilliseconds(255));
        var r = new DateDuration(TimeSpan.FromDays(2));
        var s = new byte[] {1, 2, 3, 4, 5};

        var result = await _client.QueryRequiredSingleAsync<ScalarContainer>(
            """
            WITH
                container := (INSERT tests::ScalarContainer {
                    a := 5,
                    b := 7,
                    c := 299,
                    d := 'ABC',
                    e := true,
                    f := 221.5,
                    g := 2999.999,
                    h := 19248124,
                    i := <decimal>23.2,
                    j := <uuid>$j,
                    k := <json>$k,
                    l := <datetime>$l,
                    m := <cal::local_datetime>$m,
                    n := <cal::local_date>$n,
                    o := <cal::local_time>$o,
                    p := <duration>$p,
                    q := <cal::relative_duration>$q,
                    r := <cal::date_duration>$r,
                    s := <bytes>$s
                })
            SELECT container {
                a,
                b,
                c,
                d,
                e,
                f,
                g,
                h,
                i,
                j,
                k,
                l,
                m,
                n,
                o,
                p,
                q,
                r,
                s
            }
            """, new
            {
                j,
                k,
                l,
                m,
                n,
                o,
                p,
                q,
                r,
                s
            });

        #region Null checks

        Assert.IsNotNull(result.A);
        Assert.IsNotNull(result.B);
        Assert.IsNotNull(result.C);
        Assert.IsNotNull(result.D);
        Assert.IsNotNull(result.E);
        Assert.IsNotNull(result.F);
        Assert.IsNotNull(result.G);
        Assert.IsNotNull(result.H);
        Assert.IsNotNull(result.I);
        Assert.IsNotNull(result.J);
        Assert.IsNotNull(result.K);
        Assert.IsNotNull(result.L);
        Assert.IsNotNull(result.M);
        Assert.IsNotNull(result.N);
        Assert.IsNotNull(result.O);
        Assert.IsNotNull(result.P);
        Assert.IsNotNull(result.Q);
        Assert.IsNotNull(result.R);
        Assert.IsNotNull(result.S);

        #endregion

        Assert.AreEqual(5, result.A.Value);
        Assert.AreEqual(7, result.B.Value);
        Assert.AreEqual(299, result.C.Value);
        Assert.AreEqual("ABC", result.D);
        Assert.AreEqual(true, result.E.Value);
        Assert.AreEqual(221.5f, result.F.Value);
        Assert.AreEqual(2999.999, result.G.Value);
        Assert.AreEqual(19248124, result.H.Value);
        Assert.AreEqual((decimal)23.2, result.I.Value);
        Assert.AreEqual(j, result.J.Value);
        Assert.AreEqual(k, result.K.Value);
        Assert.AreEqual(l, result.L.Value);
        Assert.AreEqual(m, result.M.Value);
        Assert.AreEqual(n, result.N.Value);
        Assert.AreEqual(o, result.O.Value);
        Assert.AreEqual(p, result.P.Value);
        Assert.AreEqual(q, result.Q.Value);
        Assert.AreEqual(r, result.R.Value);
        Assert.IsTrue(s.SequenceEqual(result.S));
    }

    public class A
    {
        public B? PropA { get; set; }
        public List<string>? ListOfString { get; set; }
        public string[]? ArrayOfString { get; set; }

        public class B
        {
            public string? PropB { get; set; }
        }
    }

    [TestMethod]
    public async Task TestNestedObjectQuery()
    {
        var result = await _client.QuerySingleAsync<A>("select { prop_a := { prop_b := \"foo\"} }");

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.PropA);
        Assert.AreEqual("foo", result.PropA.PropB);
    }

    [TestMethod]
    public async Task TestNestedCollectionQuery()
    {
        var arrArrResult = await _client.QuerySingleAsync<A>("select { array_of_string := ['foo', 'bar', 'baz'] }");

        Assert.IsNotNull(arrArrResult);
        Assert.IsNotNull(arrArrResult.ArrayOfString);
        Assert.AreEqual(3, arrArrResult.ArrayOfString.Length);
        Assert.IsTrue(arrArrResult.ArrayOfString.SequenceEqual(new[] {"foo", "bar", "baz"}));


        var listArrResult = await _client.QuerySingleAsync<A>("select { list_of_string := ['foo', 'bar', 'baz'] }");

        Assert.IsNotNull(listArrResult);
        Assert.IsNotNull(listArrResult.ListOfString);
        Assert.AreEqual(3, listArrResult.ListOfString.Count);
        Assert.IsTrue(listArrResult.ListOfString.SequenceEqual(new[] {"foo", "bar", "baz"}));

        var arrSetResult = await _client.QuerySingleAsync<A>("select { array_of_string := {'foo', 'bar', 'baz'} }");

        Assert.IsNotNull(arrSetResult);
        Assert.IsNotNull(arrSetResult.ArrayOfString);
        Assert.AreEqual(3, arrSetResult.ArrayOfString.Length);
        Assert.IsTrue(arrSetResult.ArrayOfString.SequenceEqual(new[] {"foo", "bar", "baz"}));

        var listSetResult = await _client.QuerySingleAsync<A>("select { list_of_string := {'foo', 'bar', 'baz'} }");

        Assert.IsNotNull(listSetResult);
        Assert.IsNotNull(listSetResult.ListOfString);
        Assert.AreEqual(3, listSetResult.ListOfString.Count);
        Assert.IsTrue(listSetResult.ListOfString.SequenceEqual(new[] {"foo", "bar", "baz"}));
    }

    #endregion

    #region Helpers

    private static bool CompareMicroseconds(TimeSpan a, TimeSpan b) =>
        // since the system type ('a' in this case) can be in tick-presision, round
        // as we would for standard temporals.
        Math.Round(a.TotalMicroseconds) == b.TotalMicroseconds;

    private TimeSpan RandomTimeSpan()
    {
        var ticks = Random.Shared.NextInt64(TimeSpan.Zero.Ticks, TimeSpan.FromDays(100).Ticks);
        return TimeSpan.FromTicks(ticks / 1000 * 1000); // convert to microsecond precision to match db
    }

    private DateTime RandomDateTime()
    {
        var secs = Random.Shared.NextInt64((long)(DateTime.Now - DateTime.MinValue).TotalMilliseconds);
        return DateTime.MinValue.AddMilliseconds(secs)
            .RoundToMicroseconds(); // convert to microsecond precision to match db
    }

    private DateTimeOffset RandomDTO() => RandomDateTime().ToUniversalTime().RoundToMicroseconds();

    private DateOnly RandomDate() => DateOnly.FromDateTime(RandomDateTime());

    #endregion
}
