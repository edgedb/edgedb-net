using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsAssertSingle : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "assert_single({0})";
    }
}
