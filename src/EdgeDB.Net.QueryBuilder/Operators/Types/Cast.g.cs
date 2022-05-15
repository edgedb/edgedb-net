using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesCast : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.Convert;
        public string EdgeQLOperator => "<{0}>{1}";
    }
}
