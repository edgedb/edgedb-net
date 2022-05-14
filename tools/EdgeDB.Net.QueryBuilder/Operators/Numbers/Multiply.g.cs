using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersMultiply : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Multiply;
        public string EdgeQLOperator => "{0} * {1}";
    }
}
