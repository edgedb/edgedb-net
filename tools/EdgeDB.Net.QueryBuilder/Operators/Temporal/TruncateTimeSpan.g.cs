using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalTruncateTimeSpan : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "duration_truncate({0}, {1})";
    }
}
