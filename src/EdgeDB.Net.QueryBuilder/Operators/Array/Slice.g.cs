using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class ArraySlice : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "{0}[{1}:{2?}]";
    }
}
