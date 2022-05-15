using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class LinksAddLink : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "+= {1}";
    }
}
