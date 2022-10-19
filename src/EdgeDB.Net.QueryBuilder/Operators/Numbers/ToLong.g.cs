using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersToLong : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "to_int64({0}, {1?})";
    }
}
