using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringSplit : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "str_split({0}, {1})";
    }
}
