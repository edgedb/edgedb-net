using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class LinksAddLink : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "+= {1}";
    }
}
