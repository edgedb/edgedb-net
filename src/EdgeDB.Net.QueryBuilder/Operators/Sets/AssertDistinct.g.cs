using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsAssertDistinct : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "assert_distinct({0})";
    }
}
