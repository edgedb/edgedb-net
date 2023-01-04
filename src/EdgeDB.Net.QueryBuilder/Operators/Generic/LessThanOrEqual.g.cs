using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericLessThanOrEqual : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.LessThanOrEqual;
        public string EdgeQLOperator => "{0} <= {1}";
    }
}
