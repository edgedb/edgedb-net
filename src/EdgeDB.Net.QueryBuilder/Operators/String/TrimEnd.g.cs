using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringTrimEnd : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "str_trim_end({0}, {1?})";
    }
}
