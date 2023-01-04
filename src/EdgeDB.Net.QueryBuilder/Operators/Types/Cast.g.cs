using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesCast : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Convert;
        public string EdgeQLOperator => "<{0}>{1}";
    }
}
