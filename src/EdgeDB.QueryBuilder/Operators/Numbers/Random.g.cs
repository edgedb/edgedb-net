using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersRandom : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "random()";
    }
}
