using System.Linq.Expressions;

namespace EdgeDB;

internal class EquivalentExpressionAttribute : Attribute
{
    public EquivalentExpressionAttribute(params ExpressionType[] expressions)
    {
        Expressions = expressions;
    }

    public ExpressionType[] Expressions { get; }
}
