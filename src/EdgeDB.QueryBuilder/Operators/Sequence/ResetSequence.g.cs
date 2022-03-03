using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SequenceResetSequence : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "sequence_reset(<introspect typeof {0}>, {1?})";
    }
}
