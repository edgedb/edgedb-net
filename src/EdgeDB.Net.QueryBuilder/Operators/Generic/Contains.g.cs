using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericContains : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "contains({0}, {1})";
    }
}
