using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class ArrayIndex : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.Index;
        public string EdgeQLOperator => "{0}[{1}]";
    }
}
