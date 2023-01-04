using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericGreaterThan : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.GreaterThan;
        public string EdgeQLOperator => "{0} > {1}";
    }
}
