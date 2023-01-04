using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsEnumerate : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "enumerate({0})";
    }
}
