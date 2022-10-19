using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringTrim : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "str_trim({0}, {1?})";
    }
}
