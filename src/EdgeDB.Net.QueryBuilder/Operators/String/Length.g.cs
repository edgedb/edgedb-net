using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringLength : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "len({0})";
    }
}
