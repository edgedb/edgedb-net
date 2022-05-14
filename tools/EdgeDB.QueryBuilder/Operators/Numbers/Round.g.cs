using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersRound : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "round({0}, {1?})";
    }
}
