using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersToShort : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "to_int16({0}, {1?})";
    }
}
