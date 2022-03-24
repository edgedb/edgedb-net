using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericGreaterThanOrEqual : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.GreaterThanOrEqual;
        public string EdgeQLOperator => "{0} >= {1}";
    }
}
