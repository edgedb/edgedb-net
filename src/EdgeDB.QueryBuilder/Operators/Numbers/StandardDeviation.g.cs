using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersStandardDeviation : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::stddev({0})";
    }
}
