using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsAssertSingle : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "assert_single({0})";
    }
}
