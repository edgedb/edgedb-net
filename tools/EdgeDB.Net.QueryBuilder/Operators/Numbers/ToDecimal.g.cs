using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersToDecimal : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "to_decimal({0}, {1?})";
    }
}
