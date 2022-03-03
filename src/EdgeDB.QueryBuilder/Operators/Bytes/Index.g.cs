using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BytesIndex : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Index;
        public string EdgeQLOperator => "{0}[{1}]";
    }
}
