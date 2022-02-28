using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersVariance : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::var({0})";
    }
}
