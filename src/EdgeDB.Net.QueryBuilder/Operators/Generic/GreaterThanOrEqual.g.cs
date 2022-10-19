using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericGreaterThanOrEqual : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.GreaterThanOrEqual;
        public string EdgeQLOperator => "{0} >= {1}";
    }
}
