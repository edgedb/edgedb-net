using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringLength : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "len({0})";
    }
}
