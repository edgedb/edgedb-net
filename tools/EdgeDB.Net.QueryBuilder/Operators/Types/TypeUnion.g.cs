using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesTypeUnion : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "({0} | {1} { | :2+})";
    }
}
