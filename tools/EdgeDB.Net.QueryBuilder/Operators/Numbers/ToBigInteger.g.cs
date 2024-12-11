using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersToBigInteger : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "to_bigint({0}, {1?})";
    }
}
