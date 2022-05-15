using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsDetached : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "detached {0}";
    }
}
