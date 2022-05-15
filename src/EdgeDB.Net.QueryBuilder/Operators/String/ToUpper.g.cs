using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringToUpper : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "str_upper({0})";
    }
}
