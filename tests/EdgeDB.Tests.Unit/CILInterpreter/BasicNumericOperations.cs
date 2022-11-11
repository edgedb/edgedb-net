using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace EdgeDB.Tests.Unit
{
    public partial class CILTester
    {
        [TestMethod]
        public void TestBasicNumericOperations()
        {
            RunAndCompareExpression = true;
            TranslateToEdgeQL = true;

            TestFunction(() => 1);
            TestFunction(() => 1000);
            TestFunction(() => 1u);
            TestFunction(() => 1ul);
            TestFunction(() => 1L);
            TestFunction(() => 1 + 1);
            TestFunction(() => 69 + 420 - 1337);
            TestFunction(() => 69 * 420 - 1337 / 2 % 4);
            TestFunction(() => 1 << 2);
            TestFunction(() => 1 & 2);
            TestFunction(() => 1 | 2);
        }
    }
}

