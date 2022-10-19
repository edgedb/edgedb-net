using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BytesGetBit : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "bytes_get_bit({0}, {1})";
    }
}
