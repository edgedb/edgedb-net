using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringLength : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "len({0})";
    }
}
