using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathMean : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "math::mean({0})";
    }
}
