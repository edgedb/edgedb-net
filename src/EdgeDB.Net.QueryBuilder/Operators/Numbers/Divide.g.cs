using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersDivide : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.Divide;
        public string EdgeQLOperator => "{0} / {1}";
    }
}
