using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathFloor : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "math::floor({0})";
    }
}
