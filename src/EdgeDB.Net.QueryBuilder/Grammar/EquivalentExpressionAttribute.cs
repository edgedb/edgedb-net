using System.Linq.Expressions;

namespace EdgeDB;

internal class EquivalentExpressionAttribute(params ExpressionType[] expressions) : Attribute
{
    public ExpressionType[] Expressions { get; } = expressions;
}
