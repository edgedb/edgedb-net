using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersMultiply : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.Multiply;
        public string EdgeQLOperator => "{0} * {1}";
    }
}
