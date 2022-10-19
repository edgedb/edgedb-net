using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringMatch : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "re_match({0}, {1})";
    }
}
