using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class LinksRemoveLink : IEdgeQLOperator
    {
        public ExpressionType? Operator => null;
        public string EdgeQLOperator => "-= {1}";
    }
}
