using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersSum : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "sum({0})";
    }
}
