using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalToDateTimeOffset : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "to_datetime({0}, {1?}, {2?}, {3?}, {4?}, {5?}, {6?})";
    }
}
