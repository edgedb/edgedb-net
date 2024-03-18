using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsAssertNotNull : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "assert_exists({0})";
    }
}
