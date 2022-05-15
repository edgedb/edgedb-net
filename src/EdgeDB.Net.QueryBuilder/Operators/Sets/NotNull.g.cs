using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsNotNull : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "exists {0}";
    }
}
