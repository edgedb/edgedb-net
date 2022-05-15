using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathStandardDeviation : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "math::stddev({0})";
    }
}
