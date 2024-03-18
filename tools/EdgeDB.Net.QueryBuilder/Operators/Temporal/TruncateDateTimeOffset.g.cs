using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalTruncateDateTimeOffset : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "datetime_truncate({0}, {1})";
    }
}
