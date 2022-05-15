using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringTrim : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "str_trim({0}, {1?})";
    }
}
