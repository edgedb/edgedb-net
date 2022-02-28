using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersAbs : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::abs({0})";
    }
}
