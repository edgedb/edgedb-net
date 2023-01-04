using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalGetTimespanElement : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "cal::time_get({0}, {1})";
    }
}
