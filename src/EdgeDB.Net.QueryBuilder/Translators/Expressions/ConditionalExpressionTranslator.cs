using System.Linq.Expressions;

namespace EdgeDB.Translators.Expressions;

/// <summary>
///     Represents a translator for translating an expression with a conditional operator.
/// </summary>
internal class ConditionalExpressionTranslator : ExpressionTranslator<ConditionalExpression>
{
    /// <inheritdoc />
    public override void Translate(
        ConditionalExpression expression,
        ExpressionContext context,
        QueryWriter writer) =>
        writer.Append(
            Proxy(expression.IfTrue, context),
            " IF ",
            Proxy(expression.Test, context),
            " ELSE ",
            Proxy(expression.IfFalse, context)
        );
}
