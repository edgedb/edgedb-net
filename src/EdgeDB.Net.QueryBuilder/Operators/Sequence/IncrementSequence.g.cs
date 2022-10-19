using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SequenceIncrementSequence : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "sequence_next(<introspect typeof {0}>)";
    }
}
