using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    [TestClass]
    public class V2Arguments
    {
        [TestMethod("Argument of type array<std::bigint>")]
        public Task V2Arguments_0()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\0.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bigint")]
        public Task V2Arguments_1()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\1.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int16")]
        public Task V2Arguments_10()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\10.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int32")]
        public Task V2Arguments_11()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\11.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int64")]
        public Task V2Arguments_12()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\12.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::json")]
        public Task V2Arguments_13()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\13.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_datetime")]
        public Task V2Arguments_14()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\14.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_date")]
        public Task V2Arguments_15()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\15.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_time")]
        public Task V2Arguments_16()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\16.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type range<std::int32>")]
        public Task V2Arguments_17()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\17.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::relative_duration")]
        public Task V2Arguments_18()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\18.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::str")]
        public Task V2Arguments_19()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\19.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bool")]
        public Task V2Arguments_2()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\2.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::uuid")]
        public Task V2Arguments_20()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\20.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type array<std::bigint>")]
        public Task V2Arguments_21()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\21.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bigint")]
        public Task V2Arguments_22()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\22.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bool")]
        public Task V2Arguments_23()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\23.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bytes")]
        public Task V2Arguments_24()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\24.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::date_duration")]
        public Task V2Arguments_25()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\25.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::datetime")]
        public Task V2Arguments_26()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\26.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::decimal")]
        public Task V2Arguments_27()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\27.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::duration")]
        public Task V2Arguments_28()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\28.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float32")]
        public Task V2Arguments_29()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\29.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bytes")]
        public Task V2Arguments_3()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\3.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float64")]
        public Task V2Arguments_30()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\30.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int16")]
        public Task V2Arguments_31()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\31.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int32")]
        public Task V2Arguments_32()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\32.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int64")]
        public Task V2Arguments_33()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\33.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::json")]
        public Task V2Arguments_34()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\34.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_datetime")]
        public Task V2Arguments_35()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\35.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_date")]
        public Task V2Arguments_36()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\36.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_time")]
        public Task V2Arguments_37()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\37.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type range<std::int32>")]
        public Task V2Arguments_38()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\38.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::relative_duration")]
        public Task V2Arguments_39()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\39.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::date_duration")]
        public Task V2Arguments_4()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\4.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::str")]
        public Task V2Arguments_40()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\40.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::uuid")]
        public Task V2Arguments_41()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\41.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type array<std::bigint>")]
        public Task V2Arguments_42()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\42.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bigint")]
        public Task V2Arguments_43()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\43.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bool")]
        public Task V2Arguments_44()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\44.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::bytes")]
        public Task V2Arguments_45()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\45.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::date_duration")]
        public Task V2Arguments_46()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\46.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::datetime")]
        public Task V2Arguments_47()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\47.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::decimal")]
        public Task V2Arguments_48()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\48.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::duration")]
        public Task V2Arguments_49()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\49.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::datetime")]
        public Task V2Arguments_5()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\5.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float32")]
        public Task V2Arguments_50()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\50.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float64")]
        public Task V2Arguments_51()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\51.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int16")]
        public Task V2Arguments_52()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\52.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int32")]
        public Task V2Arguments_53()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\53.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::int64")]
        public Task V2Arguments_54()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\54.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::json")]
        public Task V2Arguments_55()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\55.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_datetime")]
        public Task V2Arguments_56()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\56.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_date")]
        public Task V2Arguments_57()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\57.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::local_time")]
        public Task V2Arguments_58()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\58.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type range<std::int32>")]
        public Task V2Arguments_59()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\59.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::decimal")]
        public Task V2Arguments_6()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\6.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type cal::relative_duration")]
        public Task V2Arguments_60()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\60.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::str")]
        public Task V2Arguments_61()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\61.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::uuid")]
        public Task V2Arguments_62()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\62.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::duration")]
        public Task V2Arguments_7()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\7.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float32")]
        public Task V2Arguments_8()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\8.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Argument of type std::float64")]
        public Task V2Arguments_9()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_arguments\9.json";
            return SharedTestsRunner.RunAsync(path);
        }

    }
}
