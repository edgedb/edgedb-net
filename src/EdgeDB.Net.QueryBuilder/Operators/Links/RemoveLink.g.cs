using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class LinksRemoveLink : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "-= {0}";
    }
}
