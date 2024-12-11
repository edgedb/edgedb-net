using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static EdgeDB.Tests.Integration.QueryBuilder.Expressions.ExpressionTester;
using static System.Linq.Expressions.Expression;

namespace EdgeDB.Tests.Integration.QueryBuilder.Expressions
{
    [TestClass]
    public class BinaryExpressionTests
    {
        [TestMethod]
        public void AddExpression()
        {
            AssertExpression<int>(() => 1 + 1, "2");
            AssertExpression<int>(Add(Constant(1), Constant(1)), "1 + 1");

            // TODO: support this
            // AssertExpression<int>(AddChecked(Constant(1), Constant(2)), "1 + 2");

            AssertExpression<long>(Add(Constant(5L), Constant(5L)), "5 + 5");
        }
    }
}
