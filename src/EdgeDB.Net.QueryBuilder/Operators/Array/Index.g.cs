using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class ArrayIndex : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Index;
        public string EdgeQLOperator => "{0}[{1}]";
    }
}
