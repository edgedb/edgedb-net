using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace EdgeDB.Tests.Unit
{
    public partial class CILTester
    {
        [TestMethod]
        public void TestNestedExpressionsAndDelegates()
        {
            TranslateToEdgeQL = false;
            RunAndCompareExpression = true;
            CompileExpression = true;

            var instance = new MockBuilder();

            TestFunction(() => instance.Select(() => 1));
            TestFunction(() =>
                instance
                    .Select(() => 123)
                    .Filter(x => x > 2 || x < -1)
            );

            TestFunction(() => instance.ComplexSelect(() => new
            {
                PropA = 123,
                PropB = "456"
            }));
        }
    }

    public class MockBuilder
    {
        public MockBuilder Select(Expression<Func<int>> select) { return this; }

        public MockBuilder Filter(Expression<Func<int, bool>> filter) { return this; }

        public MockBuilder ComplexSelect(Func<object> select) { return this; }
    }
}

