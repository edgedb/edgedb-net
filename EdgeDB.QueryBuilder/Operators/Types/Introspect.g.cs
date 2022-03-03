using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesIntrospect : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "introspect {0}";
    }
}
