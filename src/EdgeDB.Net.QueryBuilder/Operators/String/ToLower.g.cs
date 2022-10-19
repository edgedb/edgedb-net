using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringToLower : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "str_lower({0})";
    }
}
