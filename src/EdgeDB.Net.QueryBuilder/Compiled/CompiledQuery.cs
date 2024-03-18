using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace EdgeDB;

public class CompiledQuery(string query, Dictionary<string, object?>? variables)
{
    public string Query { get; } = query;

    public string Prettied => Prettify();

    public IReadOnlyDictionary<string, object?>? Variables { get; }
        = variables?.ToImmutableDictionary();

    internal readonly Dictionary<string, object?>? RawVariables = variables;

    /// <summary>
    ///     Prettifies the query text.
    /// </summary>
    /// <remarks>
    ///     This method uses a lot of regex and can be unreliable, if
    ///     you're using this in a production setting please use with care.
    /// </remarks>
    /// <returns>A prettified version of <see cref="Query"/>.</returns>
    public string Prettify()
    {
        // add newlines
        var result = Regex.Replace(Query, @"({|\(|\)|}|,)", m =>
        {
            switch (m.Groups[1].Value)
            {
                case "{" or "(" or ",":
                    if (m.Groups[1].Value == "{" && Query[m.Index + 1] == '}')
                        return m.Groups[1].Value;

                    return $"{m.Groups[1].Value}\n";

                default:
                    return $"{((m.Groups[1].Value == "}" && (Query[m.Index - 1] == '{' || Query[m.Index - 1] == '}')) ? "" : "\n")}{m.Groups[1].Value}{((Query.Length != m.Index + 1 && (Query[m.Index + 1] != ',')) ? "\n" : "")}";
            }
        }).Trim().Replace("\n ", "\n");

        // clean up newline func
        result = Regex.Replace(result, "\n\n", m => "\n");

        // add indentation
        result = Regex.Replace(result, "^", m =>
        {
            int indent = 0;

            foreach (var c in result[..m.Index])
            {
                if (c is '(' or '{')
                    indent++;
                if (c is ')' or '}')
                    indent--;
            }

            var next = result.Length != m.Index ? result[m.Index] : '\0';

            if (next is '}' or ')')
                indent--;

            return "".PadLeft(indent * 2);
        }, RegexOptions.Multiline);

        return result;
    }
}
