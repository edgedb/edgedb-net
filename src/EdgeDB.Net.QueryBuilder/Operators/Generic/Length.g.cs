using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericLength : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "len({0})";
    }
}
