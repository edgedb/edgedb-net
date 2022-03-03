using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersLogarithm : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::log({0} <base := {1}>)";
    }
}
