using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalTruncateTimeSpan : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "duration_truncate({0}, {1})";
    }
}
