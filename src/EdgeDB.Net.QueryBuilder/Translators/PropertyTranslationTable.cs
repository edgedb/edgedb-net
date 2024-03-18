using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EdgeDB.Translators;

internal static class PropertyTranslationTable
{
    internal delegate void TableTranslator(QueryWriter writer, MemberExpression expression, ExpressionContext context);

    private static readonly Dictionary<string, (Type[] ValidOn, TableTranslator Translator)> Table = new()
    {
        {nameof(string.Length), ([typeof(string), typeof(Array)], TranslateLength)}
    };

    public static bool TryGetTranslator(MemberExpression expression, [MaybeNullWhen(false)] out TableTranslator translator)
    {
        if (Table.TryGetValue(expression.Member.Name, out var entry))
        {
            if (entry.ValidOn.Contains(expression.Member.DeclaringType!))
            {
                translator = entry.Translator;
                return true;
            }
        }

        translator = null;
        return false;
    }

    private static void TranslateLength(QueryWriter writer, MemberExpression expression, ExpressionContext context)
    {
        if (expression.Expression is null)
            throw new InvalidOperationException("Missing target for 'len()'");

        writer.Function(
            "std::len",
            Defer.This(() => $"Auto .Length access converted to std::len()"),
            ExpressionTranslator.Proxy(expression.Expression, context)
        );
    }
}
