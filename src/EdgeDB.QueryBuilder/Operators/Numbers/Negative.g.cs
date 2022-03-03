using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersNegative : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Negate;
        public string EdgeQLOperator => "-{0}";
    }
}
