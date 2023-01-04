using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathCeil : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "math::ceil({0})";
    }
}
