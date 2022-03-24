using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalToRelativeDuration : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "cal::to_relative_duration(<years := {0?}>, <months := {1?}>, <days := {2?}>, <hours := {3?}>, <minutes := {4?}>, <seconds := {5?}>, <microseconds := {6?}>)";
    }
}
