using System.Linq.Expressions;

namespace EdgeDB.Translators.Expressions;

internal sealed class ListInitExpressionTranslator : ExpressionTranslator<ListInitExpression>
{
    public override void Translate(ListInitExpression expression, ExpressionContext context, QueryWriter writer) =>
        writer.Wrapped(writer =>
        {
            for (var i = 0; i != expression.Initializers.Count - 1; i++)
            {
                WriteInitializer(writer, expression.Initializers[i], context);
                writer.Append(", ");
            }

            WriteInitializer(writer, expression.Initializers[^1], context);
        }, "{}");

    private void WriteInitializer(QueryWriter writer, ElementInit initializer, ExpressionContext context)
    {
        if (initializer.Arguments.Count == 1)
        {
            writer.Append(Proxy(initializer.Arguments[0], context));
        }
        else
        {
            writer.Wrapped(Token.Of(writer =>
            {
                for (var i = 0; i < initializer.Arguments.Count - 1; i++)
                {
                    writer.Append(Proxy(initializer.Arguments[i], context), ", ");
                }

                writer.Append(Proxy(initializer.Arguments[^1], context));
            }));
        }
    }
}
