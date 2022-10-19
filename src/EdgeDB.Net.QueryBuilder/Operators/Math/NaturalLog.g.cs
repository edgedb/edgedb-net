using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathNaturalLog : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "math::ln({0})";
    }
}
