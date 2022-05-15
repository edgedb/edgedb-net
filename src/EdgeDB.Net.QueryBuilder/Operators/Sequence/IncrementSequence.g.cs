using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SequenceIncrementSequence : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "sequence_next(<introspect typeof {0}>)";
    }
}
