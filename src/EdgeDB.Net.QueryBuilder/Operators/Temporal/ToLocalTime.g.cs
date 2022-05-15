using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalToLocalTime : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "cal::to_local_time({0}, {1?}, {2?})";
    }
}
