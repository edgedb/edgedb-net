using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringToTitle : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "str_title({0})";
    }
}
