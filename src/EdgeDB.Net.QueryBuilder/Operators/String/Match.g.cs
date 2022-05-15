using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringMatch : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "re_match({0}, {1})";
    }
}
