using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsMin : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "min({0})";
    }
}
