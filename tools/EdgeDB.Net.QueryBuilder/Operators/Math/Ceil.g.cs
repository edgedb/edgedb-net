using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathCeil : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::ceil({0})";
    }
}
