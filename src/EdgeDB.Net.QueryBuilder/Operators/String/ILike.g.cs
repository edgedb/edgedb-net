using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringILike : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "{0} ilike {1}";
    }
}
