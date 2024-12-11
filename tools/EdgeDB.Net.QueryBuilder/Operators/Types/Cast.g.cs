using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesCast : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Convert;
        public string EdgeQLOperator => "<{0}>{1}";
    }
}
