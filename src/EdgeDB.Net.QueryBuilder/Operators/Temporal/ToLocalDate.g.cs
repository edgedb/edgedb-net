using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalToLocalDate : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "cal::to_local_date({0}, {1?}, {2?})";
    }
}
