using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersDivide : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Divide;
        public string EdgeQLOperator => "{0} / {1}";
    }
}
