using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BooleanNot : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Not;
        public string EdgeQLOperator => "not {0}";
    }
}
