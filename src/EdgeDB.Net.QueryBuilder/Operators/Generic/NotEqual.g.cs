using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericNotEqual : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.NotEqual;
        public string EdgeQLOperator => "{0} ?!= {1}";
    }
}
