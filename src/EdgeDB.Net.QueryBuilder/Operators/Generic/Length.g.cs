using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class GenericLength : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "len({0})";
    }
}
