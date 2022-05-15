using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathStandardDeviationPop : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "math::stddev_pop({0})";
    }
}
