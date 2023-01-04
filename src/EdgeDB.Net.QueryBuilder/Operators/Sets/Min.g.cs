using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsMin : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "min({0})";
    }
}
