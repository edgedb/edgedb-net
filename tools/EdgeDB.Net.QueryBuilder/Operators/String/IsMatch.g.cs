using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringIsMatch : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "re_test({0}, {1})";
    }
}
