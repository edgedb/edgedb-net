using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericFind : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "find({0}, {1})";
    }
}
