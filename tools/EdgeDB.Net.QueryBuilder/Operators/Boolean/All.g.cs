using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BooleanAll : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "all({0})";
    }
}
