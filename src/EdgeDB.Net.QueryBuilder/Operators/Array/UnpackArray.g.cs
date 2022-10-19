using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class ArrayUnpackArray : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "array_unpack({0})";
    }
}
