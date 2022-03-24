using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsCount : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "count({0})";
    }
}
