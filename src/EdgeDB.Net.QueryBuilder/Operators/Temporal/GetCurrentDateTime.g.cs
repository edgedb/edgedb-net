using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalGetCurrentDateTime : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "std::datetime_current()";
    }
}
