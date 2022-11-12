using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace EdgeDB.Tests.Unit
{
    public partial class CILTester
    {
        [TestMethod]
        public void TestIfStatements()
        {
            RunAndCompareExpression = true;
            CompileExpression = true;

            var ternary = (bool x) => x ? 1 : 0;
            var ifstatement = (bool x) =>
            {
                if (x)
                {
                    return 123;
                }

                return 456;
            };

            var ifElseStatement = (bool x) =>
            {
                if (x)
                    return 123;
                else
                    return 456;
            };

            var chainedIfElse = (int num) =>
            {
                if (num < 0)
                    return 1234;
                else if (num > 0)
                    return 4321;
                else
                    return 888;
            };

            TestFunction(ternary, true);
            TestFunction(ternary, false);

            TestFunction(ifstatement, true);
            TestFunction(ifstatement, false);

            TestFunction(ifElseStatement, true);
            TestFunction(ifElseStatement, false);

            TestFunction(chainedIfElse, -1);
            TestFunction(chainedIfElse, -888);
            TestFunction(chainedIfElse, 123);
            TestFunction(chainedIfElse, 0);
        }
    }
}

