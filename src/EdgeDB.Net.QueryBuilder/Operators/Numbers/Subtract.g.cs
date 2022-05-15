using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersSubtract : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.Subtract;
        public string EdgeQLOperator => "{0} - {1}";
    }
}
