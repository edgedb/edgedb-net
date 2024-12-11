using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringToLower : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "str_lower({0})";
    }
}
