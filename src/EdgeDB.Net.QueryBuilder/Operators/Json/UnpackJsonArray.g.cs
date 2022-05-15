using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class JsonUnpackJsonArray : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "json_array_unpack({0})";
    }
}
