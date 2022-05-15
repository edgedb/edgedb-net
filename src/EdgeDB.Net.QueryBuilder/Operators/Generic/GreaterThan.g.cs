using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericGreaterThan : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.GreaterThan;
        public string EdgeQLOperator => "{0} > {1}";
    }
}
