using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class VariablesReference : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "{0}";
    }
}
