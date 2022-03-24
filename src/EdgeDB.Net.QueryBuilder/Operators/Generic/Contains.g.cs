using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericContains : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "contains({0}, {1})";
    }
}
