using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersModulo : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.Modulo;
        public string EdgeQLOperator => "{0} % {1}";
    }
}
