using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersAdd : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Add;
        public string EdgeQLOperator => "{0} + {1}";
    }
}
