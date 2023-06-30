using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericNotEqual : IEdgeQLOperator
    {
        public ExpressionType? Operator => ExpressionType.NotEqual;
        public string EdgeQLOperator => "{0} ?!= {1}";
    }
}
