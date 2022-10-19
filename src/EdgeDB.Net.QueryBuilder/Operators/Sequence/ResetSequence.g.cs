using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SequenceResetSequence : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "sequence_reset(<introspect typeof {0}>, {1?})";
    }
}
