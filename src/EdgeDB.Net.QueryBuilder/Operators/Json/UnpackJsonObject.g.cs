using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class JsonUnpackJsonObject : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "json_object_unpack({0})";
    }
}
