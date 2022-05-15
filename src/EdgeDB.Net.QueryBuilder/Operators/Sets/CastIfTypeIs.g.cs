using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsCastIfTypeIs : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.TypeIs;
        public string EdgeQLOperator => "{0}[is {1}]";
    }
}
