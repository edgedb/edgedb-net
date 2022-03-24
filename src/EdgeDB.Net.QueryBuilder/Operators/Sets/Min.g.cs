using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsMin : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "min({0})";
    }
}
