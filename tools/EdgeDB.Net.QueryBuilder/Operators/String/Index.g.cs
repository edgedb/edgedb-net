using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringIndex : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Index;
        public string EdgeQLOperator => "{0}[{1}]";
    }
}
