using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class BooleanAny : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "any({0})";
    }
}
