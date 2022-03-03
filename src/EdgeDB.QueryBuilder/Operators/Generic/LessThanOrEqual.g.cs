using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericLessThanOrEqual : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.LessThanOrEqual;
        public string EdgeQLOperator => "{0} <= {1}";
    }
}
