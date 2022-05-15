using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathFloor : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "math::floor({0})";
    }
}
