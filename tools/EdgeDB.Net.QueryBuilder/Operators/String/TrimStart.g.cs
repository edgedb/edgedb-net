using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringTrimStart : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "str_trim_start({0}, {1?})";
    }
}
