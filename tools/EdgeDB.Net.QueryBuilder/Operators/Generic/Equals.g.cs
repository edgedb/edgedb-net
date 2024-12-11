using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericEquals : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.Equal;
        public string EdgeQLOperator => "{0} ?= {1}";
    }
}
