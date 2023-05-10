using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    [TestClass]
    public class V2QueryResults
    {
        [TestMethod("Query result of type array<cal::local_date>")]
        public Task V2QueryResults_0()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\0.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::uuid>")]
        public Task V2QueryResults_1()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\1.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::datetime>")]
        public Task V2QueryResults_10()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\10.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::json")]
        public Task V2QueryResults_100()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\100.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type cal::local_datetime")]
        public Task V2QueryResults_101()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\101.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type cal::local_date")]
        public Task V2QueryResults_102()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\102.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type cal::local_time")]
        public Task V2QueryResults_103()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\103.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::uuid>")]
        public Task V2QueryResults_104()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\104.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::int16, std::bytes, std::float64, cal::date_duration, namedtuple<std::decimal, std::int16>, namedtuple<std::int16, std::decimal>, namedtuple<std::int16, std::decimal>, cal::local_time, array<cal::local_datetime>, array<std::datetime>, array<std::decimal>, array<std::float32>, array<std::uuid>, array<std::uuid>, array<std::float64>, array<std::int64>, array<std::int64>, array<std::int32>, array<std::int16>, array<std::str>, array<cal::local_date>, array<std::json>, array<std::json>, array<std::bytes>, array<cal::relative_duration>>")]
        public Task V2QueryResults_105()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\105.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::bytes, std::float64, cal::date_duration, namedtuple<std::uuid, std::bytes, std::decimal>, namedtuple<std::uuid, std::bytes, std::decimal>, namedtuple<std::bytes, std::decimal, std::uuid>, namedtuple<std::decimal, std::uuid, std::bytes>, namedtuple<std::uuid, std::bytes, std::decimal>, cal::local_time, array<cal::relative_duration>, array<std::float64>, array<std::float64>, array<std::json>, array<std::bool>, array<cal::local_time>, array<cal::local_time>, array<std::int64>, array<cal::date_duration>, array<cal::local_date>, array<std::str>, array<std::bytes>, array<std::datetime>, array<std::duration>, std::int16>")]
        public Task V2QueryResults_106()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\106.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::float64, cal::date_duration, namedtuple<cal::relative_duration>, namedtuple<cal::relative_duration>, namedtuple<cal::relative_duration>, cal::local_time, array<std::bytes>, array<std::json>, array<std::int16>, array<std::float64>, array<std::str>, array<std::str>, array<std::int32>, array<cal::local_datetime>, array<std::decimal>, array<std::bigint>, array<cal::local_date>, std::int16, std::bytes>")]
        public Task V2QueryResults_107()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\107.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<cal::date_duration, namedtuple<std::bool, cal::local_datetime, std::decimal, std::float32, std::int16, std::float64, cal::local_date>, namedtuple<cal::local_datetime, std::decimal, std::float32, std::int16, std::float64, cal::local_date, std::bool>, namedtuple<std::decimal, std::float32, std::int16, std::float64, cal::local_date, std::bool, cal::local_datetime>, namedtuple<std::float32, std::int16, std::float64, cal::local_date, std::bool, cal::local_datetime, std::decimal>, namedtuple<std::int16, std::float64, cal::local_date, std::bool, cal::local_datetime, std::decimal, std::float32>, namedtuple<std::int16, std::float64, cal::local_date, std::bool, cal::local_datetime, std::decimal, std::float32>, namedtuple<std::float64, cal::local_date, std::bool, cal::local_datetime, std::decimal, std::float32, std::int16>, namedtuple<cal::local_date, std::bool, cal::local_datetime, std::decimal, std::float32, std::int16, std::float64>, cal::local_time, array<std::json>, array<std::int16>, array<std::bytes>, array<std::bytes>, array<std::decimal>, array<std::float64>, array<std::datetime>, array<cal::local_datetime>, array<std::duration>, array<std::duration>, array<std::duration>, array<cal::date_duration>, std::int16, std::bytes, std::float64>")]
        public Task V2QueryResults_108()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\108.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<namedtuple<std::bigint, std::int32, cal::local_time, std::json, cal::relative_duration>, namedtuple<std::int32, cal::local_time, std::json, cal::relative_duration, std::bigint>, namedtuple<cal::local_time, std::json, cal::relative_duration, std::bigint, std::int32>, namedtuple<cal::local_time, std::json, cal::relative_duration, std::bigint, std::int32>, namedtuple<cal::local_time, std::json, cal::relative_duration, std::bigint, std::int32>, namedtuple<std::json, cal::relative_duration, std::bigint, std::int32, cal::local_time>, namedtuple<std::json, cal::relative_duration, std::bigint, std::int32, cal::local_time>, namedtuple<cal::relative_duration, std::bigint, std::int32, cal::local_time, std::json>, cal::local_time, array<std::float64>, array<std::str>, array<std::str>, array<std::bigint>, array<cal::local_date>, std::int16, std::bytes, std::float64, cal::date_duration>")]
        public Task V2QueryResults_109()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\109.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::int64>")]
        public Task V2QueryResults_11()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\11.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<cal::local_time, array<std::json>, array<std::decimal>, array<cal::local_datetime>, array<std::bigint>, array<std::bool>, array<std::float64>, array<std::int64>, array<std::int64>, array<cal::date_duration>, array<cal::date_duration>, array<std::uuid>, array<cal::relative_duration>, array<cal::local_date>, array<cal::local_date>, array<std::datetime>, array<std::datetime>, array<std::str>, array<std::int32>, array<std::float32>, std::int16, std::bytes, std::float64, cal::date_duration>")]
        public Task V2QueryResults_110()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\110.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<array<std::str>, array<std::float64>, array<std::int32>, array<std::decimal>, array<cal::local_time>, array<cal::local_time>, std::int16, std::bytes, std::float64, cal::date_duration, namedtuple<std::decimal, std::float64, std::bigint, std::json>, namedtuple<std::float64, std::bigint, std::json, std::decimal>, namedtuple<std::bigint, std::json, std::decimal, std::float64>, namedtuple<std::json, std::decimal, std::float64, std::bigint>, namedtuple<std::decimal, std::float64, std::bigint, std::json>, cal::local_time>")]
        public Task V2QueryResults_111()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\111.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<std::str>")]
        public Task V2QueryResults_112()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\112.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<std::datetime, std::uuid, cal::local_date>")]
        public Task V2QueryResults_113()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\113.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<std::uuid, cal::local_date, std::datetime>")]
        public Task V2QueryResults_114()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\114.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<cal::local_date, std::datetime, std::uuid>")]
        public Task V2QueryResults_115()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\115.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type range<cal::local_date>")]
        public Task V2QueryResults_116()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\116.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type range<std::int32>")]
        public Task V2QueryResults_117()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\117.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type cal::relative_duration")]
        public Task V2QueryResults_118()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\118.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::int16>")]
        public Task V2QueryResults_119()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\119.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::bool>")]
        public Task V2QueryResults_12()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\12.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<cal::relative_duration>")]
        public Task V2QueryResults_120()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\120.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<range<std::int64>>")]
        public Task V2QueryResults_121()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\121.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::bigint>")]
        public Task V2QueryResults_122()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\122.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<cal::local_datetime>")]
        public Task V2QueryResults_123()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\123.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::uuid>")]
        public Task V2QueryResults_124()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\124.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<cal::date_duration>")]
        public Task V2QueryResults_125()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\125.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<set<std::datetime>>")]
        public Task V2QueryResults_126()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\126.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::str>")]
        public Task V2QueryResults_127()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\127.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::bool>")]
        public Task V2QueryResults_128()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\128.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::json>")]
        public Task V2QueryResults_129()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\129.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::bool>")]
        public Task V2QueryResults_13()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\13.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::str")]
        public Task V2QueryResults_130()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\130.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type tuple<cal::local_datetime, std::json>")]
        public Task V2QueryResults_131()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\131.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type tuple<std::json, cal::local_datetime>")]
        public Task V2QueryResults_132()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\132.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::uuid")]
        public Task V2QueryResults_133()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\133.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type array<std::int64>")]
        public Task V2QueryResults_134()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\134.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type cal::local_time")]
        public Task V2QueryResults_135()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\135.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type namedtuple<std::int64, cal::local_time, std::str>")]
        public Task V2QueryResults_136()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\136.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type namedtuple<std::int64, cal::local_time, std::str>")]
        public Task V2QueryResults_137()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\137.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type object<std::int64, cal::local_time, std::str>")]
        public Task V2QueryResults_138()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\138.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type object<std::int64, cal::local_time, std::str>")]
        public Task V2QueryResults_139()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\139.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<cal::relative_duration>")]
        public Task V2QueryResults_14()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\14.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type range<std::int64>")]
        public Task V2QueryResults_140()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\140.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type set<std::int64>")]
        public Task V2QueryResults_141()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\141.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type tuple<std::int64, cal::local_time, std::str>")]
        public Task V2QueryResults_142()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\142.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type array<std::int64>")]
        public Task V2QueryResults_143()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\143.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type cal::local_time")]
        public Task V2QueryResults_144()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\144.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>")]
        public Task V2QueryResults_145()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\145.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>")]
        public Task V2QueryResults_146()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\146.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>")]
        public Task V2QueryResults_147()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\147.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>")]
        public Task V2QueryResults_148()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\148.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type range<std::int64>")]
        public Task V2QueryResults_149()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\149.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::bigint")]
        public Task V2QueryResults_15()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\15.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type set<array<std::int64>>")]
        public Task V2QueryResults_150()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\150.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>")]
        public Task V2QueryResults_151()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\151.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type array<std::int64>")]
        public Task V2QueryResults_152()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\152.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type cal::local_time")]
        public Task V2QueryResults_153()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\153.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>")]
        public Task V2QueryResults_154()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\154.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>")]
        public Task V2QueryResults_155()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\155.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>")]
        public Task V2QueryResults_156()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\156.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>")]
        public Task V2QueryResults_157()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\157.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type range<std::int64>")]
        public Task V2QueryResults_158()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\158.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type set<array<std::int64>>")]
        public Task V2QueryResults_159()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\159.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::bool")]
        public Task V2QueryResults_16()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\16.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>")]
        public Task V2QueryResults_160()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\160.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type array<std::int64>")]
        public Task V2QueryResults_161()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\161.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type cal::local_time")]
        public Task V2QueryResults_162()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\162.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>>")]
        public Task V2QueryResults_163()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\163.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>>")]
        public Task V2QueryResults_164()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\164.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>>")]
        public Task V2QueryResults_165()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\165.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>>")]
        public Task V2QueryResults_166()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\166.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type range<std::int64>")]
        public Task V2QueryResults_167()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\167.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type set<array<std::int64>>")]
        public Task V2QueryResults_168()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\168.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Deep query nesting of type tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, namedtuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, object<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>, range<std::int64>, set<array<std::int64>>, std::str, tuple<array<std::int64>, std::int64, cal::local_time, namedtuple<std::int64, cal::local_time, std::str>, namedtuple<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, object<std::int64, cal::local_time, std::str>, range<std::int64>, set<std::int64>, std::str, tuple<std::int64, cal::local_time, std::str>>>>")]
        public Task V2QueryResults_169()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\169.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::bytes")]
        public Task V2QueryResults_17()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\17.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type cal::date_duration")]
        public Task V2QueryResults_18()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\18.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::datetime")]
        public Task V2QueryResults_19()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\19.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<cal::date_duration>")]
        public Task V2QueryResults_2()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\2.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::decimal")]
        public Task V2QueryResults_20()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\20.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::duration")]
        public Task V2QueryResults_21()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\21.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::float32")]
        public Task V2QueryResults_22()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\22.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::float64")]
        public Task V2QueryResults_23()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\23.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::int16")]
        public Task V2QueryResults_24()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\24.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::int32")]
        public Task V2QueryResults_25()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\25.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::int64")]
        public Task V2QueryResults_26()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\26.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::json")]
        public Task V2QueryResults_27()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\27.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type cal::local_datetime")]
        public Task V2QueryResults_28()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\28.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type cal::local_date")]
        public Task V2QueryResults_29()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\29.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::float64>")]
        public Task V2QueryResults_3()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\3.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type cal::local_time")]
        public Task V2QueryResults_30()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\30.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::int32, cal::date_duration, std::datetime, std::str>")]
        public Task V2QueryResults_31()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\31.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<cal::date_duration, std::datetime, std::str, std::int32>")]
        public Task V2QueryResults_32()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\32.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::datetime, std::str, std::int32, cal::date_duration>")]
        public Task V2QueryResults_33()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\33.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::datetime, std::str, std::int32, cal::date_duration>")]
        public Task V2QueryResults_34()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\34.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::datetime, std::str, std::int32, cal::date_duration>")]
        public Task V2QueryResults_35()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\35.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::str, std::int32, cal::date_duration, std::datetime>")]
        public Task V2QueryResults_36()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\36.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::int32, cal::date_duration, std::datetime, std::str>")]
        public Task V2QueryResults_37()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\37.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::int32, cal::relative_duration, std::json, std::bigint, std::float32>")]
        public Task V2QueryResults_38()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\38.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<cal::relative_duration, std::json, std::bigint, std::float32, std::int32>")]
        public Task V2QueryResults_39()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\39.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::int16>")]
        public Task V2QueryResults_4()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\4.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::json, std::bigint, std::float32, std::int32, cal::relative_duration>")]
        public Task V2QueryResults_40()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\40.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::json, std::bigint, std::float32, std::int32, cal::relative_duration>")]
        public Task V2QueryResults_41()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\41.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::bigint, std::float32, std::int32, cal::relative_duration, std::json>")]
        public Task V2QueryResults_42()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\42.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type namedtuple<std::float32, std::int32, cal::relative_duration, std::json, std::bigint>")]
        public Task V2QueryResults_43()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\43.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<std::int32, std::str, cal::local_datetime, std::bool, std::decimal, std::bigint>")]
        public Task V2QueryResults_44()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\44.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<std::str, cal::local_datetime, std::bool, std::decimal, std::bigint, std::int32>")]
        public Task V2QueryResults_45()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\45.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<cal::local_datetime, std::bool, std::decimal, std::bigint, std::int32, std::str>")]
        public Task V2QueryResults_46()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\46.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<std::bool, std::decimal, std::bigint, std::int32, std::str, cal::local_datetime>")]
        public Task V2QueryResults_47()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\47.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<std::bool, std::decimal, std::bigint, std::int32, std::str, cal::local_datetime>")]
        public Task V2QueryResults_48()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\48.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<std::decimal, std::bigint, std::int32, std::str, cal::local_datetime, std::bool>")]
        public Task V2QueryResults_49()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\49.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::duration>")]
        public Task V2QueryResults_5()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\5.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type object<std::bigint, std::int32, std::str, cal::local_datetime, std::bool, std::decimal>")]
        public Task V2QueryResults_50()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\50.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type range<std::int32>")]
        public Task V2QueryResults_51()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\51.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type range<std::int64>")]
        public Task V2QueryResults_52()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\52.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type range<std::float32>")]
        public Task V2QueryResults_53()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\53.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type range<std::float64>")]
        public Task V2QueryResults_54()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\54.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type range<std::decimal>")]
        public Task V2QueryResults_55()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\55.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type range<std::datetime>")]
        public Task V2QueryResults_56()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\56.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type range<cal::local_datetime>")]
        public Task V2QueryResults_57()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\57.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type range<cal::local_date>")]
        public Task V2QueryResults_58()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\58.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type cal::relative_duration")]
        public Task V2QueryResults_59()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\59.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::duration>")]
        public Task V2QueryResults_6()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\6.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<cal::local_time>")]
        public Task V2QueryResults_60()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\60.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<cal::date_duration>")]
        public Task V2QueryResults_61()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\61.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<cal::date_duration>")]
        public Task V2QueryResults_62()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\62.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<cal::date_duration>")]
        public Task V2QueryResults_63()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\63.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::int16>")]
        public Task V2QueryResults_64()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\64.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::int32>")]
        public Task V2QueryResults_65()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\65.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::json>")]
        public Task V2QueryResults_66()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\66.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::bool>")]
        public Task V2QueryResults_67()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\67.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::bool>")]
        public Task V2QueryResults_68()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\68.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<std::uuid>")]
        public Task V2QueryResults_69()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\69.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::int32>")]
        public Task V2QueryResults_7()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\7.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<cal::local_date>")]
        public Task V2QueryResults_70()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\70.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<cal::local_date>")]
        public Task V2QueryResults_71()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\71.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type set<cal::local_date>")]
        public Task V2QueryResults_72()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\72.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::str")]
        public Task V2QueryResults_73()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\73.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type tuple<cal::local_time, std::duration>")]
        public Task V2QueryResults_74()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\74.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type tuple<cal::local_time, std::duration>")]
        public Task V2QueryResults_75()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\75.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type tuple<std::duration, cal::local_time>")]
        public Task V2QueryResults_76()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\76.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type tuple<std::duration, cal::local_time>")]
        public Task V2QueryResults_77()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\77.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::uuid")]
        public Task V2QueryResults_78()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\78.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<range<std::int32>>")]
        public Task V2QueryResults_79()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\79.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::json>")]
        public Task V2QueryResults_8()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\8.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::str>")]
        public Task V2QueryResults_80()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\80.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::int16>")]
        public Task V2QueryResults_81()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\81.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::int64>")]
        public Task V2QueryResults_82()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\82.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<tuple<std::float32, cal::local_time, std::json, std::str, std::uuid, std::int64, std::duration, std::float64>>")]
        public Task V2QueryResults_83()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\83.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::float64>")]
        public Task V2QueryResults_84()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\84.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<cal::date_duration>")]
        public Task V2QueryResults_85()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\85.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<namedtuple<std::datetime, std::duration, std::str, cal::date_duration, std::uuid, std::float32>>")]
        public Task V2QueryResults_86()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\86.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::datetime>")]
        public Task V2QueryResults_87()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\87.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::bigint")]
        public Task V2QueryResults_88()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\88.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::bool")]
        public Task V2QueryResults_89()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\89.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type array<std::bytes>")]
        public Task V2QueryResults_9()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\9.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::bytes")]
        public Task V2QueryResults_90()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\90.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type cal::date_duration")]
        public Task V2QueryResults_91()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\91.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::datetime")]
        public Task V2QueryResults_92()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\92.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::decimal")]
        public Task V2QueryResults_93()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\93.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::duration")]
        public Task V2QueryResults_94()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\94.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::float32")]
        public Task V2QueryResults_95()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\95.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::float64")]
        public Task V2QueryResults_96()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\96.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::int16")]
        public Task V2QueryResults_97()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\97.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::int32")]
        public Task V2QueryResults_98()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\98.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Query result of type std::int64")]
        public Task V2QueryResults_99()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_query_results\99.json";
            return SharedTestsRunner.RunAsync(path);
        }

    }
}
