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

            TestFunction(() => new int[] { 1, 2, 3 });
            TestFunction(() => new string[] { "a", "b", "c" });

            TranslateToEdgeQL = false;

            TestFunction(() => TestParams("1", "ab", "cd"));

            TestFunction(() =>
            {
                var c = new TestClass();
                return c.ToString();
            });

        }

        public void TestParams(params string[] args) { }

        public class TestClass
        {

        }
    }
}

