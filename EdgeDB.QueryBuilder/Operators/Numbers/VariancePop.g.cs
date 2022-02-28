using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersVariancePop : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::var_pop({0})";
    }
}
