using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathStandardDeviation : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "math::stddev({0})";
    }
}
