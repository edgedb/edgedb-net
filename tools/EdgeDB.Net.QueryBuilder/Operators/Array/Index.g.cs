using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class ArrayIndex : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Index;
        public string EdgeQLOperator => "{0}[{1}]";
    }
}
