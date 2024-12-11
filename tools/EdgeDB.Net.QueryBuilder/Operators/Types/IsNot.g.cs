using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesIsNot : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "{0} is not {1}";
    }
}
