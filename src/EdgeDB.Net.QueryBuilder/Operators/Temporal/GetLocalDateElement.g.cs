using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalGetLocalDateElement : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "cal::date_get({0}, {1})";
    }
}
