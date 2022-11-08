using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringMatchAll : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "re_match_all({0}, {1})";
    }
}