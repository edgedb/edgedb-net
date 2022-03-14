using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class VariablesReference : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "{0}";
    }
}
