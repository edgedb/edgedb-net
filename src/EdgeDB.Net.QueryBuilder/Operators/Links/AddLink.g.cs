using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class LinksAddLink : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "+= {0}";
    }
}
