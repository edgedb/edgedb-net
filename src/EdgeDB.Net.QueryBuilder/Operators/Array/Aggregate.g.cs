using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class ArrayAggregate : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "array_agg({0})";
    }
}
