using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BooleanAll : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "all({0})";
    }
}
