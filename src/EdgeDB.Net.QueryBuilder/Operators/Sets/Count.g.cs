using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsCount : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "count({0})";
    }
}
