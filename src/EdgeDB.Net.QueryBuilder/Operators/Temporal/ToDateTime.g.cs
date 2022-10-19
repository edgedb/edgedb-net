using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalToDateTime : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "cal::to_local_datetime({0}, {1?}, {2?}, {3?}, {4?}, {5?})";
    }
}
