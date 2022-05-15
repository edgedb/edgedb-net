using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringToLower : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "str_lower({0})";
    }
}
