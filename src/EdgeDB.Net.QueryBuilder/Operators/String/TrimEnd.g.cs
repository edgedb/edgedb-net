using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringTrimEnd : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "str_trim_end({0}, {1?})";
    }
}
