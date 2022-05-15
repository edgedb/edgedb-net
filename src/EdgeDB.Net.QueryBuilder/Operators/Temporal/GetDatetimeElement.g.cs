using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalGetDatetimeElement : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "datetime_get({0}, {1})";
    }
}
