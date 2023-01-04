using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesGetType : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "introspect (typeof {0})";
    }
}
