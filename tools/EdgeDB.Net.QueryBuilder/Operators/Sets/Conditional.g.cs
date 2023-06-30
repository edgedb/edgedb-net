using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsConditional : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Conditional;
        public string EdgeQLOperator => "{1} if {0} else {2}";
    }
}
