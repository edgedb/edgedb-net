using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringToString : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "to_str({0})";
    }
}
