using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersModulo : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Modulo;
        public string EdgeQLOperator => "";
    }
}
