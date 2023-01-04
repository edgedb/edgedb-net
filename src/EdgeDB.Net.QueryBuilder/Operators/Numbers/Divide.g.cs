using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersDivide : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Divide;
        public string EdgeQLOperator => "{0} / {1}";
    }
}
