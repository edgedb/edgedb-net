using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
    public class CodecConversionTests
    {
        private readonly EdgeDBClient _client;

        public CodecConversionTests()
        {
            _client = ClientProvider.EdgeDB;
        }

        private async Task TestTypeQuerying<TType>(string tname, TType expected, Predicate<TType>? comparer = null)
        {
            var actual = await _client.QueryRequiredSingleAsync<TType>($"select <{tname}>$value", new Dictionary<string, object?>
            {
                {"value", expected }
            });

            if(comparer is not null)
            {
                Assert.IsTrue(comparer(actual));
            }
            else if (expected is byte[] bt && actual is byte[] bt2)
            {
                Assert.IsTrue(bt.SequenceEqual(bt2));
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

        #region Arrays
        private async Task TestArrayQuerying<T>(string tname, T[] expected)
            => await TestTypeQuerying<T[]>($"array<{tname}>", expected, v => v.SequenceEqual(expected));
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
        }

        public async Task TestArrayOfRangeScalars()
        {

        }
        #endregion

        #region Ranges
        private async Task TestRangeQuerying<T>(string tname, DataTypes.Range<T> expected)
            where T : struct
            => await TestTypeQuerying<DataTypes.Range<T>>($"range<{tname}>", expected);

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
            => TestRangeQuerying("datetime", new DataTypes.Range<DateTime>(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow));

        [TestMethod]
        public Task TestRangeOfSystemDateTimeOffset()
            => TestRangeQuerying("datetime", new DataTypes.Range<DateTimeOffset>(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow));

        [TestMethod]
        public Task TestRangeOfLocalDateTime()
            => TestRangeQuerying("cal::local_datetime", new DataTypes.Range<DataTypes.LocalDateTime>(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow));

        [TestMethod]
        public Task TestRangeOfSystemLocalDateTime()
            => TestRangeQuerying("cal::local_datetime", new DataTypes.Range<DateTime>(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow));

        [TestMethod]
        public Task TestRangeOfSystemLocalDateTimeOffset()
            => TestRangeQuerying("cal::local_datetime", new DataTypes.Range<DateTimeOffset>(DateTime.UtcNow.AddDays(-2), DateTime.UtcNow));

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
            => TestTypeQuerying("cal::local_time", TimeOnly.FromDateTime(DateTime.UtcNow));

        [TestMethod]
        public Task TestDateOnlyLegacy()
            => TestTypeQuerying("cal::local_date", RandomDate());
        #endregion

        #region Helpers
        private TimeSpan RandomTimeSpan()
        {
            var ticks = Random.Shared.NextInt64(TimeSpan.Zero.Ticks, TimeSpan.FromDays(100).Ticks);
            return TimeSpan.FromTicks((ticks / 1000) * 1000); // convert to microsecond precision to match db
        }

        private DateTime RandomDateTime()
        {
            var ticks = Random.Shared.NextInt64(new DateTime(2000, 1, 1, 0, 0, 0).Ticks, DateTime.UtcNow.Ticks);
            return new DateTime((ticks / 1000) * 1000).ToUniversalTime(); // convert to microsecond precision to match db
        }

        private DateTimeOffset RandomDTO()
        {
            return RandomDateTime().ToUniversalTime();
        }

        private DateOnly RandomDate()
        {
            return DateOnly.FromDateTime(RandomDateTime());
        }
        #endregion
    }
}
