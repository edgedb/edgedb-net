using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    [TestClass]
    public class V3Arguments
    {
        [TestMethod("Argument of type array<std::bigint>")]
        public Task V3Arguments_0()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\0.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bigint")]
        public Task V3Arguments_1()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\1.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int16")]
        public Task V3Arguments_10()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\10.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int32")]
        public Task V3Arguments_11()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\11.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int64")]
        public Task V3Arguments_12()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\12.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::json")]
        public Task V3Arguments_13()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\13.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_datetime")]
        public Task V3Arguments_14()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\14.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_date")]
        public Task V3Arguments_15()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\15.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_time")]
        public Task V3Arguments_16()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\16.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type range<std::int32>")]
        public Task V3Arguments_17()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\17.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::relative_duration")]
        public Task V3Arguments_18()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\18.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::str")]
        public Task V3Arguments_19()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\19.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bool")]
        public Task V3Arguments_2()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\2.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type tuple<std::bigint, std::bool, std::bytes, cal::date_duration, std::datetime, std::decimal, std::duration, std::float32, std::float64, std::int16, std::int32, std::int64, std::json, cal::local_datetime, cal::local_date, cal::local_time, cal::relative_duration, std::str, std::uuid>")]
        public Task V3Arguments_20()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\20.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::uuid")]
        public Task V3Arguments_21()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\21.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type array<std::bigint>")]
        public Task V3Arguments_22()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\22.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bigint")]
        public Task V3Arguments_23()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\23.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bool")]
        public Task V3Arguments_24()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\24.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bytes")]
        public Task V3Arguments_25()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\25.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::date_duration")]
        public Task V3Arguments_26()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\26.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::datetime")]
        public Task V3Arguments_27()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\27.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::decimal")]
        public Task V3Arguments_28()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\28.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::duration")]
        public Task V3Arguments_29()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\29.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bytes")]
        public Task V3Arguments_3()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\3.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float32")]
        public Task V3Arguments_30()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\30.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float64")]
        public Task V3Arguments_31()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\31.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int16")]
        public Task V3Arguments_32()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\32.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int32")]
        public Task V3Arguments_33()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\33.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int64")]
        public Task V3Arguments_34()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\34.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::json")]
        public Task V3Arguments_35()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\35.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_datetime")]
        public Task V3Arguments_36()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\36.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_date")]
        public Task V3Arguments_37()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\37.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_time")]
        public Task V3Arguments_38()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\38.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type range<std::int32>")]
        public Task V3Arguments_39()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\39.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::date_duration")]
        public Task V3Arguments_4()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\4.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::relative_duration")]
        public Task V3Arguments_40()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\40.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::str")]
        public Task V3Arguments_41()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\41.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type tuple<array<std::bigint>, std::bigint, std::bool, std::bytes, cal::date_duration, std::datetime, std::decimal, std::duration, std::float32, std::float64, std::int16, std::int32, std::int64, std::json, cal::local_datetime, cal::local_date, cal::local_time, range<std::int32>, cal::relative_duration, std::str, tuple<std::bigint, std::bool, std::bytes, cal::date_duration, std::datetime, std::decimal, std::duration, std::float32, std::float64, std::int16, std::int32, std::int64, std::json, cal::local_datetime, cal::local_date, cal::local_time, cal::relative_duration, std::str, std::uuid>, std::uuid>")]
        public Task V3Arguments_42()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\42.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::uuid")]
        public Task V3Arguments_43()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\43.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type array<std::bigint>")]
        public Task V3Arguments_44()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\44.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bigint")]
        public Task V3Arguments_45()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\45.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bool")]
        public Task V3Arguments_46()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\46.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bytes")]
        public Task V3Arguments_47()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\47.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::date_duration")]
        public Task V3Arguments_48()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\48.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::datetime")]
        public Task V3Arguments_49()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\49.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::datetime")]
        public Task V3Arguments_5()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\5.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::decimal")]
        public Task V3Arguments_50()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\50.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::duration")]
        public Task V3Arguments_51()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\51.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float32")]
        public Task V3Arguments_52()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\52.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float64")]
        public Task V3Arguments_53()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\53.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int16")]
        public Task V3Arguments_54()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\54.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int32")]
        public Task V3Arguments_55()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\55.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int64")]
        public Task V3Arguments_56()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\56.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::json")]
        public Task V3Arguments_57()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\57.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_datetime")]
        public Task V3Arguments_58()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\58.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_date")]
        public Task V3Arguments_59()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\59.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::decimal")]
        public Task V3Arguments_6()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\6.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_time")]
        public Task V3Arguments_60()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\60.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type range<std::int32>")]
        public Task V3Arguments_61()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\61.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::relative_duration")]
        public Task V3Arguments_62()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\62.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::str")]
        public Task V3Arguments_63()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\63.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type tuple<array<std::bigint>, std::bigint, std::bool, std::bytes, cal::date_duration, std::datetime, std::decimal, std::duration, std::float32, std::float64, std::int16, std::int32, std::int64, std::json, cal::local_datetime, cal::local_date, cal::local_time, range<std::int32>, cal::relative_duration, std::str, tuple<array<std::bigint>, std::bigint, std::bool, std::bytes, cal::date_duration, std::datetime, std::decimal, std::duration, std::float32, std::float64, std::int16, std::int32, std::int64, std::json, cal::local_datetime, cal::local_date, cal::local_time, range<std::int32>, cal::relative_duration, std::str, tuple<std::bigint, std::bool, std::bytes, cal::date_duration, std::datetime, std::decimal, std::duration, std::float32, std::float64, std::int16, std::int32, std::int64, std::json, cal::local_datetime, cal::local_date, cal::local_time, cal::relative_duration, std::str, std::uuid>, std::uuid>, std::uuid>")]
        public Task V3Arguments_64()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\64.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::uuid")]
        public Task V3Arguments_65()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\65.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type array<std::bigint>")]
        public Task V3Arguments_66()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\66.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bigint")]
        public Task V3Arguments_67()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\67.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bool")]
        public Task V3Arguments_68()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\68.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bytes")]
        public Task V3Arguments_69()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\69.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::duration")]
        public Task V3Arguments_7()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\7.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::date_duration")]
        public Task V3Arguments_70()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\70.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::datetime")]
        public Task V3Arguments_71()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\71.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::decimal")]
        public Task V3Arguments_72()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\72.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::duration")]
        public Task V3Arguments_73()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\73.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float32")]
        public Task V3Arguments_74()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\74.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float64")]
        public Task V3Arguments_75()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\75.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int16")]
        public Task V3Arguments_76()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\76.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int32")]
        public Task V3Arguments_77()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\77.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int64")]
        public Task V3Arguments_78()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\78.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::json")]
        public Task V3Arguments_79()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\79.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float32")]
        public Task V3Arguments_8()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\8.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_datetime")]
        public Task V3Arguments_80()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\80.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_date")]
        public Task V3Arguments_81()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\81.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_time")]
        public Task V3Arguments_82()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\82.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type range<std::int32>")]
        public Task V3Arguments_83()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\83.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::relative_duration")]
        public Task V3Arguments_84()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\84.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::str")]
        public Task V3Arguments_85()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\85.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type tuple<array<std::bigint>, std::bigint, std::bool, std::bytes, cal::date_duration, std::datetime, std::decimal, std::duration, std::float32, std::float64, std::int16, std::int32, std::int64, std::json, cal::local_datetime, cal::local_date, cal::local_time, range<std::int32>, cal::relative_duration, std::str, tuple<array<std::bigint>, std::bigint, std::bool, std::bytes, cal::date_duration, std::datetime, std::decimal, std::duration, std::float32, std::float64, std::int16, std::int32, std::int64, std::json, cal::local_datetime, cal::local_date, cal::local_time, range<std::int32>, cal::relative_duration, std::str, tuple<array<std::bigint>, std::bigint, std::bool, std::bytes, cal::date_duration, std::datetime, std::decimal, std::duration, std::float32, std::float64, std::int16, std::int32, std::int64, std::json, cal::local_datetime, cal::local_date, cal::local_time, range<std::int32>, cal::relative_duration, std::str, tuple<std::bigint, std::bool, std::bytes, cal::date_duration, std::datetime, std::decimal, std::duration, std::float32, std::float64, std::int16, std::int32, std::int64, std::json, cal::local_datetime, cal::local_date, cal::local_time, cal::relative_duration, std::str, std::uuid>, std::uuid>, std::uuid>, std::uuid>")]
        public Task V3Arguments_86()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\86.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::uuid")]
        public Task V3Arguments_87()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\87.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float64")]
        public Task V3Arguments_9()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v3_arguments\9.json";
            return SharedTestsRunner.RunAsync(path);
        }

    }
}
