using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsContains : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "{1} in {0}";
    }
}
