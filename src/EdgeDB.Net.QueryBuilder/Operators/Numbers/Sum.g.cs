using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersSum : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "sum({0})";
    }
}
