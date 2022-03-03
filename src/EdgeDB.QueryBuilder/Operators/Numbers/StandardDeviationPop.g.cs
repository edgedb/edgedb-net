using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersStandardDeviationPop : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::stddev_pop({0})";
    }
}
