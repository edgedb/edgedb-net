using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersRandom : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "random()";
    }
}
