using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringToUpper : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "str_upper({0})";
    }
}
