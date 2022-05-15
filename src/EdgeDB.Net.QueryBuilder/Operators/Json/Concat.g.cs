using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class JsonConcat : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "{0} ++ {1}";
    }
}
