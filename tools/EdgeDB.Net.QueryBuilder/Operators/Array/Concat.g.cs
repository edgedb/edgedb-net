using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class ArrayConcat : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "{0} ++ {1}";
    }
}