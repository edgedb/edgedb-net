using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsMax : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "max({0})";
    }
}
