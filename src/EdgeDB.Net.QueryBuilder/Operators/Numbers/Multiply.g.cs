using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersMultiply : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Multiply;
        public string EdgeQLOperator => "{0} * {1}";
    }
}
