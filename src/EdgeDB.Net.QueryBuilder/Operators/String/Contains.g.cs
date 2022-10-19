using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringContains : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "contains({0}, {1})";
    }
}
