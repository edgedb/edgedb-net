using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace EdgeDB.Tests.Unit
{
    public partial class CILTester
    {
        [TestMethod]
        public void TestLambdaBodies()
        {
            CompileExpression = true;
            RunAndCompareExpression = true;

            TestFunction(() =>
            {
                return 1;
            });

            TestFunction((int x) =>
            {
                return x + 1;
            }, 2);

            TestFunction(() =>
            {
                int x = 2;
                int y = 3;
                return x * y;
            });
        }
    }
}

