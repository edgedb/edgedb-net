using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersToDouble : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "to_float64({0}, {1?})";
    }
}
