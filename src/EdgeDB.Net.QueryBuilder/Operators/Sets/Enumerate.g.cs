using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsEnumerate : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "enumerate({0})";
    }
}
