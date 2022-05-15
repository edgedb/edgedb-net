using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathNaturalLog : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "math::ln({0})";
    }
}
