using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersCeil : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::ceil({0})";
    }
}
