using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersNegative : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.Negate;
        public string EdgeQLOperator => "-{0}";
    }
}
