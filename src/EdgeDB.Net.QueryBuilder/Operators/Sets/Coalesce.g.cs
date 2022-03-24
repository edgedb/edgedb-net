using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsCoalesce : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Coalesce;
        public string EdgeQLOperator => "{0} ?? {1}";
    }
}
