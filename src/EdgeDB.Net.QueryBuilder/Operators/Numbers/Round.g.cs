using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersRound : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "round({0}, {1?})";
    }
}
