using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BooleanAnd : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.And;
        public string EdgeQLOperator => "{0} and {1}";
    }
}
