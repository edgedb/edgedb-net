using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersFloor : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "{0} // {1}";
    }
}
