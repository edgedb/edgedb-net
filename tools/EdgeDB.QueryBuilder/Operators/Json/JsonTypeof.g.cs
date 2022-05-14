using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class JsonJsonTypeof : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "json_typeof({0})";
    }
}
