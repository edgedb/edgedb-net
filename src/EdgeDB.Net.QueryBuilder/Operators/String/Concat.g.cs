using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringConcat : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "{0} ++ {1}";
    }
}
