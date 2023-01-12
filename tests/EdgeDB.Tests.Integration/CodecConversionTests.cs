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

        private async Task TestTypeQuerying<TType>(string tname, TType expected)
        {
            var actual = await _client.QueryRequiredSingleAsync<TType>($"select <{tname}>$value", new Dictionary<string, object?>
            {
                {"value", expected }
            });

            if (expected is byte[] bt && actual is byte[] bt2)
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
            => TestTypeQuerying<DataTypes.DateTime>("datetime", DateTime.UtcNow);

        [TestMethod]
        public Task TestDuration()
            => TestTypeQuerying<DataTypes.Duration>("duration", TimeSpan.FromSeconds(Random.Shared.Next()));

        [TestMethod]
        public Task TestJson()
            => TestTypeQuerying<DataTypes.Json>("json", new("{\"name\": \"test\"}"));

        [TestMethod]
        public Task TestLocalDateTime()
            => TestTypeQuerying<DataTypes.LocalDateTime>("cal::local_datetime", DateTime.UtcNow);

        [TestMethod]
        public Task TestLocalDate()
            => TestTypeQuerying<DataTypes.LocalDate>("cal::local_date", DateOnly.FromDateTime(DateTime.UtcNow));

        [TestMethod]
        public Task TestLocalTime()
            => TestTypeQuerying<DataTypes.LocalTime>("cal::local_time", TimeOnly.FromDateTime(DateTime.UtcNow));

        [TestMethod]
        public Task TestBigInt()
            => TestTypeQuerying<BigInteger>("bigint", Random.Shared.NextInt64());

        [TestMethod]
        public Task TestRelativeDuration()
            => TestTypeQuerying<DataTypes.RelativeDuration>("cal::relative_duration", TimeSpan.FromSeconds(Random.Shared.Next()));

        [TestMethod]
        public Task TestDateDuration()
            => TestTypeQuerying<DataTypes.DateDuration>("cal::date_duration", TimeSpan.FromDays(Random.Shared.Next(999)));

        [TestMethod]
        public Task TestMemory()
            => TestTypeQuerying<DataTypes.Memory>("cfg::memory", new(Random.Shared.NextInt64()));

        [TestMethod]
        public Task TestDateTimeLegacy()
            => TestTypeQuerying("cal::local_datetime", DateTime.UtcNow);

        [TestMethod]
        public Task TestDateTimeOffsetLegacy()
            => TestTypeQuerying("datetime", DateTimeOffset.UtcNow);

        [TestMethod]
        public Task TestTimespanLegacy()
            => TestTypeQuerying("cal::local_time", TimeSpan.FromSeconds(Random.Shared.Next()));

        [TestMethod]
        public Task TestDateOnlyLegacy()
            => TestTypeQuerying("cal::local_date", DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
