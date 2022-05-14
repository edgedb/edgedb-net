using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsNotNull : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "exists {0}";
    }
}
