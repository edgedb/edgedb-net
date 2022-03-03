using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersNaturalLog : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::ln({0})";
    }
}
