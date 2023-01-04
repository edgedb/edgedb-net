using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsCoalesce : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Coalesce;
        public string EdgeQLOperator => "{0} ?? {1}";
    }
}
