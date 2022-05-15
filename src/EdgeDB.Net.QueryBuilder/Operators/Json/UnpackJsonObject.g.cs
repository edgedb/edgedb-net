using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class JsonUnpackJsonObject : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "json_object_unpack({0})";
    }
}
