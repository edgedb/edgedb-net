using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericLessThan : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.LessThan;
        public string EdgeQLOperator => "{0} < {1}";
    }
}
