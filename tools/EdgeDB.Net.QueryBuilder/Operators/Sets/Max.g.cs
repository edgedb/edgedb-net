using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsMax : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "max({0})";
    }
}
