using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathVariancePop : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::var_pop({0})";
    }
}
