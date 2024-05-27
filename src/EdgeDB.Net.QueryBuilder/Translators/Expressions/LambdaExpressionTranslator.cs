using System.Linq.Expressions;

namespace EdgeDB.Translators.Expressions;

/// <summary>
///     Represents a translator for translating a lambda expression.
/// </summary>
internal class LambdaExpressionTranslator : ExpressionTranslator<LambdaExpression>
{
    /// <inheritdoc />
    public override void Translate(LambdaExpression expression, ExpressionContext context, QueryWriter result)
    {
        // create a new context and translate the body of the lambda.
        var newContext =
            new ExpressionContext(context.NodeContext, expression, context.QueryArguments, context.Globals);

        newContext.ParameterPrefixes = context.ParameterPrefixes;
        newContext.ParameterAliases = context.ParameterAliases;

        TranslateExpression(expression.Body, newContext, result);
    }
}
