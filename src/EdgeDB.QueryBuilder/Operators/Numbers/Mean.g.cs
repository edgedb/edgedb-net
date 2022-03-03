using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersMean : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "math::mean({0})";
    }
}
