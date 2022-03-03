using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class JsonToJson : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "to_json({0})";
    }
}
