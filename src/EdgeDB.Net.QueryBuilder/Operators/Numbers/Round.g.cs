using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersRound : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "round({0}, {1?})";
    }
}
