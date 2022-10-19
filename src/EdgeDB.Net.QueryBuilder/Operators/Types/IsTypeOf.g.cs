using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesIsTypeOf : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "{0} is typeof {1}";
    }
}
