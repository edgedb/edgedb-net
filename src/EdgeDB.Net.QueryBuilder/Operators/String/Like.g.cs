using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringLike : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "{0} like {1}";
    }
}
