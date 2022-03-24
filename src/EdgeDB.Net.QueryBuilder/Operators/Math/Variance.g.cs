using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathVariance : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::var({0})";
    }
}
