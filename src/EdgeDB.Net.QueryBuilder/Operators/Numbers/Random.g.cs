using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersRandom : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "random()";
    }
}
