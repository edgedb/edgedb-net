using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersModulo : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Modulo;
        public string EdgeQLOperator => "{0} % {1}";
    }
}
