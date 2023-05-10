using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Integration.SharedTests
{
    [TestClass]
    public class V2State
    {
        [TestMethod("Global of type array<std::bigint>")]
        public Task V2State_0()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\0.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::bigint")]
        public Task V2State_1()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\1.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::int16")]
        public Task V2State_10()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\10.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::int32")]
        public Task V2State_11()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\11.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::int64")]
        public Task V2State_12()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\12.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::json")]
        public Task V2State_13()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\13.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::local_datetime")]
        public Task V2State_14()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\14.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::local_date")]
        public Task V2State_15()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\15.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::local_time")]
        public Task V2State_16()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\16.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type range<std::int32>")]
        public Task V2State_17()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\17.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::relative_duration")]
        public Task V2State_18()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\18.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::str")]
        public Task V2State_19()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\19.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::bool")]
        public Task V2State_2()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\2.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::uuid")]
        public Task V2State_20()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\20.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type array<std::bigint>")]
        public Task V2State_21()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\21.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::bigint")]
        public Task V2State_22()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\22.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::bool")]
        public Task V2State_23()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\23.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::bytes")]
        public Task V2State_24()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\24.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::date_duration")]
        public Task V2State_25()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\25.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::datetime")]
        public Task V2State_26()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\26.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::decimal")]
        public Task V2State_27()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\27.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::duration")]
        public Task V2State_28()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\28.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::float32")]
        public Task V2State_29()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\29.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::bytes")]
        public Task V2State_3()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\3.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::float64")]
        public Task V2State_30()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\30.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::int16")]
        public Task V2State_31()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\31.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::int32")]
        public Task V2State_32()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\32.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::int64")]
        public Task V2State_33()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\33.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::json")]
        public Task V2State_34()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\34.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::local_datetime")]
        public Task V2State_35()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\35.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::local_date")]
        public Task V2State_36()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\36.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::local_time")]
        public Task V2State_37()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\37.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type range<std::int32>")]
        public Task V2State_38()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\38.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::relative_duration")]
        public Task V2State_39()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\39.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::date_duration")]
        public Task V2State_4()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\4.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::str")]
        public Task V2State_40()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\40.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::uuid")]
        public Task V2State_41()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\41.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type array<std::bigint>")]
        public Task V2State_42()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\42.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::bigint")]
        public Task V2State_43()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\43.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::bool")]
        public Task V2State_44()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\44.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::bytes")]
        public Task V2State_45()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\45.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::date_duration")]
        public Task V2State_46()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\46.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::datetime")]
        public Task V2State_47()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\47.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::decimal")]
        public Task V2State_48()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\48.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::duration")]
        public Task V2State_49()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\49.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::datetime")]
        public Task V2State_5()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\5.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::float32")]
        public Task V2State_50()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\50.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::float64")]
        public Task V2State_51()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\51.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::int16")]
        public Task V2State_52()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\52.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::int32")]
        public Task V2State_53()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\53.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::int64")]
        public Task V2State_54()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\54.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::json")]
        public Task V2State_55()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\55.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::local_datetime")]
        public Task V2State_56()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\56.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::local_date")]
        public Task V2State_57()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\57.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::local_time")]
        public Task V2State_58()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\58.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type range<std::int32>")]
        public Task V2State_59()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\59.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::decimal")]
        public Task V2State_6()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\6.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type cal::relative_duration")]
        public Task V2State_60()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\60.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::str")]
        public Task V2State_61()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\61.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::uuid")]
        public Task V2State_62()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\62.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::duration")]
        public Task V2State_7()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\7.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::float32")]
        public Task V2State_8()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\8.json";
            return SharedTestsRunner.RunAsync(path);
        }

        [TestMethod("Global of type std::float64")]
        public Task V2State_9()
        {
            var path = @"C:\Users\lynch\source\repos\EdgeDB\tests\EdgeDB.Tests.Integration\tests\v2_state\9.json";
            return SharedTestsRunner.RunAsync(path);
        }

    }
}
