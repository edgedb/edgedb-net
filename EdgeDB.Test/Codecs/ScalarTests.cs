using EdgeDB.Codecs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.UnitTest.Codecs
{
    [TestClass]
    public class ScalarTests
    {
        public void TestCodec<TType>(IScalerCodec<TType>? codec, TType expectedValue, byte[] expectedSerializedValue)
        {
            Assert.IsNotNull(codec);

            try
            {
                var result = codec.Serialize(expectedValue);

                Assert.IsTrue(result.SequenceEqual(expectedSerializedValue));
            }
            catch (NotSupportedException) { }

            try
            {
                var deserialziedResult = codec.Deserialize(expectedSerializedValue);

                if (deserialziedResult is byte[] arr && expectedValue is byte[] arr2)
                    Assert.IsTrue(arr.SequenceEqual(arr2));
                else
                    Assert.AreEqual(deserialziedResult, expectedValue);
            }
            catch (NotSupportedException) { }
        }

        [TestMethod]
        public void TestBooleanCodec()
        {
            var codec = ICodec.GetScalerCodec<bool>();

            TestCodec(codec, true, new byte[] { 0x01 });
            TestCodec(codec, false, new byte[] { 0x00 });
        }

        [TestMethod]
        public void TestBytesCodec()
        {
            var codec = ICodec.GetScalerCodec<byte[]>();

            var data = new byte[] { 0x00, 0x01, 0x02, 0x03 };

            TestCodec(codec, data, data);
        }

        [TestMethod]
        public void TestDatetimeCodec()
        {
            var codec = ICodec.GetScalerCodec<DateTimeOffset>();

            var data = new byte[] { 0x00, 0x02, 0x2b, 0x35, 0x9b, 0xc4, 0x10, 0x00, };
            var expected = DateTimeOffset.Parse("2019-05-06T12:00+00:00");

            TestCodec(codec, expected, data);
        }

        [TestMethod]
        public void TestDecimalCodec()
        {
            var codec = ICodec.GetScalerCodec<decimal>();

            var data = new byte[]
            {
                // ndigits
                0x00, 0x04,
                
                // weight
                0x00, 0x01,
                
                // sign
                0x40, 0x00,
                
                // dscale
                0x00, 0x07,
                
                // digits
                0x00, 0x01, 0x13, 0x88, 0x18, 0x6a, 0x00, 0x00
            };

            decimal expected = (decimal)-15000.6250000;

            TestCodec(codec, expected, data);
        }

        [TestMethod]
        public void TestFloat32()
        {
            var codec = ICodec.GetScalerCodec<float>();

            var data = new byte[]
            {
                0xc1, 0x7a, 0x00, 0x00,
            };

            var expected = -15.625f;

            TestCodec(codec, expected, data);
        }

        [TestMethod]
        public void TestFloat64()
        {
            var codec = ICodec.GetScalerCodec<double>();

            var data = new byte[]
            {
                0xc0, 0x2f, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00,
            };

            var expected = -15.625d;

            TestCodec(codec, expected, data);
        }

        [TestMethod]
        public void TestInt16()
        {
            var codec = ICodec.GetScalerCodec<short>();

            var data = new byte[]
            {
                0x19, 0x9c
            };

            short expected = 6556;

            TestCodec(codec, expected, data);
        }

        [TestMethod]
        public void TestInt32()
        {
            var codec = ICodec.GetScalerCodec<int>();

            var data = new byte[]
            {
                0x00, 0x0a, 0x01, 0x31
            };

            int expected = 655665;

            TestCodec(codec, expected, data);
        }

        [TestMethod]
        public void TestInt64()
        {
            var codec = ICodec.GetScalerCodec<long>();

            var data = new byte[]
            {
                0x01, 0xb6, 0x9b, 0x4b, 0xe0, 0x52, 0xfa, 0xb1
            };

            long expected = 123456789987654321;

            TestCodec(codec, expected, data);
        }

        [TestMethod]
        public void TestJson()
        {
            var codec = ICodec.GetScalerCodec<Models.DataTypes.Json>();

            var data = new byte[]
            {
                // format
                0x01,
                // data
                123, 32, 34, 72, 101, 108, 108, 111, 34, 58, 32, 34, 87, 111, 114, 108, 100, 33, 34, 125,
            };

            var expected = new Models.DataTypes.Json("{ \"Hello\": \"World!\"}");

            TestCodec(codec, expected, data);
        }

        [TestMethod]
        public void TestMemory()
        {
            var codec = ICodec.GetScalerCodec<Models.DataTypes.Memory>();

            var data = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x07, 0xb0, 0x00, 0x00
            };

            var expected = new Models.DataTypes.Memory(128974848);

            TestCodec(codec, expected, data);
        }

        [TestMethod]
        public void TestText()
        {
            var codec = ICodec.GetScalerCodec<string>();

            var data = new byte[]
            {
                72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33
            };

            var expected = "Hello World!";

            TestCodec(codec, expected, data);
        }

        [TestMethod]
        public void TestUUID()
        {
            var codec = ICodec.GetScalerCodec<Guid>();

            var data = new byte[]
            {
                0xb9, 0x54, 0x5c, 0x35, 0x1f, 0xe7, 0x48, 0x5f, 0xa6, 0xea, 0xf8, 0xea, 0xd2, 0x51, 0xab, 0xd3
            };

            var expected = Guid.Parse("b9545c35-1fe7-485f-a6ea-f8ead251abd3");

            TestCodec(codec, expected, data);
        }
    }
}
