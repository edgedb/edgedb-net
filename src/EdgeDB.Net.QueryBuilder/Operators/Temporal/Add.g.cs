using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalAdd : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Add;
        public string EdgeQLOperator => "{0} + {1}";
    }
}
