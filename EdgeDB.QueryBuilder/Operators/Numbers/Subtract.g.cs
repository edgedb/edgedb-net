using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersSubtract : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Subtract;
        public string EdgeQLOperator => "{0} - {1}";
    }
}
