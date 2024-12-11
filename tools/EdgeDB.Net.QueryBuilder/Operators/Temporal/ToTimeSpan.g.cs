using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalToTimeSpan : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "to_duration(<hours := {0?}>, <minutes := {1?}>, <seconds := {2?}> <microseconds := {3?}>)";
    }
}
