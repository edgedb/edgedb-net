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
            TestFunction(() => 1u + 1);
            TestFunction(() => 500u - 499);
            TestFunction(() => 50ul * 200ul);
            TestFunction(() => (50u / 10u) % 2);
        }
    }
}

