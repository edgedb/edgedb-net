using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BooleanOr : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Or;
        public string EdgeQLOperator => "{0} or {1}";
    }
}
