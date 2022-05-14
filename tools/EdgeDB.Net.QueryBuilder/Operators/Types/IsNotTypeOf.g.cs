using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesIsNotTypeOf : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "{0} is not typeof {1}";
    }
}
