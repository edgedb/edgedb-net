using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class QueryBenchmarks
    {
        public static EdgeDBClient Client = new();
        public const string TemporalQuery =
"""
select {
    dt := datetime_current(),
    dur := <duration>'45.6 seconds',
    ldt := <cal::local_datetime>'2018-05-07T15:01:22.306916',
    ld := <cal::local_date>'2018-05-07',
    lt := <cal::local_time>'15:01:22.306916',
    rdur := <cal::relative_duration>'15 milliseconds',
    ddur := <cal::date_duration>'5 days'
}
""";

        [Benchmark]
        public Task<string?> SimpleQuery()
            => Client.QuerySingleAsync<string>("select \"Hello, World!\"");

        [Benchmark]
        public Task<ComplexQueryWithVarietyType> ComplexQueryWithVariety()
            => Client.QueryRequiredSingleAsync<ComplexQueryWithVarietyType>("select { a := 1234, b := [1, 2, 4], c := { ca := datetime_current(), cb := 'abc' }, d := (5, 10), e := range(1, 10)}");
        [Benchmark]
        public Task<EdgeDBTemporalsType> EdgeDBTemporals()
            => Client.QueryRequiredSingleAsync<EdgeDBTemporalsType>(TemporalQuery);
        [Benchmark]
        public Task<SystemTemproalsType> SystemTemporals()
            => Client.QueryRequiredSingleAsync<SystemTemproalsType>(TemporalQuery);

        public record ComplexQueryWithVarietyC(EdgeDB.DataTypes.DateTime ca, string cb);
        public record ComplexQueryWithVarietyType(long a, long[] b, ComplexQueryWithVarietyC c, (long, long) d, EdgeDB.DataTypes.Range<long> e);
        public record EdgeDBTemporalsType(
            DataTypes.DateTime dt,
            DataTypes.Duration dur,
            DataTypes.LocalDateTime ldt,
            DataTypes.LocalDate ld,
            DataTypes.LocalTime lt,
            DataTypes.RelativeDuration rdur,
            DataTypes.DateDuration ddur);

        public record SystemTemproalsType(
            DateTime dt,
            TimeSpan dur,
            DateTimeOffset ldt,
            DateOnly ld,
            TimeOnly lt,
            TimeSpan rdur,
            TimeSpan ddur);

    }
}
