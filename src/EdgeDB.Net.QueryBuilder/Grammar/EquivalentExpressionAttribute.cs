using System.Linq.Expressions;

namespace EdgeDB
{
    internal class EquivalentExpressionAttribute : Attribute
    {
        public ExpressionType[] Expressions { get; }
        public EquivalentExpressionAttribute(params ExpressionType[] expressions)
        {
            Expressions = expressions;
        }

    }
}
