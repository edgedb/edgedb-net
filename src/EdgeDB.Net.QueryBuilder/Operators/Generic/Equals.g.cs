using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericEquals : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Equal;
        public string EdgeQLOperator => "{0} ?= {1}";
    }
}
