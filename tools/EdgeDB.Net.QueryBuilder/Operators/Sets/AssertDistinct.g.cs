using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsAssertDistinct : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "assert_distinct({0})";
    }
}
