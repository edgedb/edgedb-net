using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsDetached : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "detached {0}";
    }
}
