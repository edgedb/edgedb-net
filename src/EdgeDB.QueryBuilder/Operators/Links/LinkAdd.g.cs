using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class LinksLinkAdd : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "+= {1}";
    }
}
