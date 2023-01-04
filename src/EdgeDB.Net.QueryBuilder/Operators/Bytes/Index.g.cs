using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BytesIndex : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Index;
        public string EdgeQLOperator => "{0}[{1}]";
    }
}
