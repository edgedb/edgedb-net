using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace EdgeDB.Tests.Unit
{
    public partial class CILTester
    {
        [TestMethod]
        public void TestDifferentTypes()
        {
            CompileExpression = true;
            RunAndCompareExpression = true;
            TranslateToEdgeQL = true;

            TestFunction(() => "Hello, World!");
            TestFunction(() => $"Hello, {("Wor" + "ld!")}");

            TranslateToEdgeQL = false;

            TestFunction(() =>
            {
                var c = new TestClass();
                return c.ToString();
            });

        }

        public class TestClass
        {

        }
    }
}

