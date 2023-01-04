using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericNotEqual : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.NotEqual;
        public string EdgeQLOperator => "{0} ?!= {1}";
    }
}
