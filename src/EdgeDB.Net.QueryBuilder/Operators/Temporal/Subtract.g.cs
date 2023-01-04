using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalSubtract : IEdgeQLOperator
    {
        public ExpressionType? Expression => ExpressionType.Subtract;
        public string EdgeQLOperator => "{0} - {1}";
    }
}
