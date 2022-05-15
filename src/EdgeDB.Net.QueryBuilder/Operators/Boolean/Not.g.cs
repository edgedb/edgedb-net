using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BooleanNot : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.Not;
        public string EdgeQLOperator => "not {0}";
    }
}
