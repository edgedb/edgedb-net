using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsAssertSingle : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "assert_single({0})";
    }
}
