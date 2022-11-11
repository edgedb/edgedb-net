using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace EdgeDB.Tests.Unit
{
    public partial class CILTester
    {
        [TestMethod]
        public void TestUnsignedNumbers()
        {
            // Unsigned numbers in CIL are implicitly converted
            // from signed to unsigned without changing bits.

            TestFunction(() => 1u);
            TestFunction(() => 1ul);
            TestFunction(() => DummyUIntFunc(1u));
            TestFunction(() =>
            {
                uint x = 1;
                ulong y = 1234;
            });
        }

        public void DummyUIntFunc(uint t) { }
    }
}

