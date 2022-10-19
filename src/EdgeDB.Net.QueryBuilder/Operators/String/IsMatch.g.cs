using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringIsMatch : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "re_test({0}, {1})";
    }
}
