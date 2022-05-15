using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericLessThanOrEqual : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.LessThanOrEqual;
        public string EdgeQLOperator => "{0} <= {1}";
    }
}
