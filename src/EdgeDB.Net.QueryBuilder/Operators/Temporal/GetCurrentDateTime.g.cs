using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalGetCurrentDateTime : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "std::datetime_current()";
    }
}
