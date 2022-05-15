using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesGetType : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "introspect (typeof {0})";
    }
}
