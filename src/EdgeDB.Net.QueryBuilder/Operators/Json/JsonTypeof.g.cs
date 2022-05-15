using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class JsonJsonTypeof : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "json_typeof({0})";
    }
}
