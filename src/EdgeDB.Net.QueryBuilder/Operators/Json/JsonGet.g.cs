using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class JsonJsonGet : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "json_get({0}, {1})";
    }
}
