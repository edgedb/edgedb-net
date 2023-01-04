using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathLogarithm : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "math::log({0} <base := {1}>)";
    }
}
