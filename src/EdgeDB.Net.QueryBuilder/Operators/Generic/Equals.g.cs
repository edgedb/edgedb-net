using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericEquals : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.Equal;
        public string EdgeQLOperator => "{0} ?= {1}";
    }
}
