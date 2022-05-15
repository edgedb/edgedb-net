using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericFind : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "find({0}, {1})";
    }
}
