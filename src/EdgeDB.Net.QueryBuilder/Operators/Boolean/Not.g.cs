using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BooleanNot : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Not;
        public string EdgeQLOperator => "not {0}";
    }
}
