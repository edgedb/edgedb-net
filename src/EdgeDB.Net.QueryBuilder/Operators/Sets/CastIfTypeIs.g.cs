using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsCastIfTypeIs : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.TypeIs;
        public string EdgeQLOperator => "{0}[is {1}]";
    }
}
