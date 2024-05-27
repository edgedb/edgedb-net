using System.Diagnostics;
using System.Text;

namespace EdgeDB.Compiled;

[DebuggerDisplay("{DebugView}")]
public sealed class DebugCompiledQuery : CompiledQuery
{
    internal DebugCompiledQuery(string query, Dictionary<string, object?> variables, LinkedList<QuerySpan> terms,
        LinkedList<int> tokens)
        : base(query, variables)
    {
        DebugView = CreateDebugText(query, variables, terms, tokens);
    }

    public string DebugView { get; }

    private static string CreateDebugText(string query, Dictionary<string, object?> variables,
        LinkedList<QuerySpan> terms, LinkedList<int> tokens)
    {
        var sb = new StringBuilder();

        if (terms.Count > 0)
        {
            StringBuilder? topRow = null;

            var view = CreateTermView(terms);
            var termTexts = new Dictionary<string, QuerySpan>();
            var rows = new List<StringBuilder>();

            foreach (var row in view)
            {
                var rowText = new StringBuilder($"{"".PadLeft(query.Length)}\n{"".PadLeft(query.Length)}");

                foreach (var column in row)
                {
                    var size = column.Range.End.Value - column.Range.Start.Value;

                    if (size <= 2)
                    {
                        topRow ??= new StringBuilder("".PadLeft(query.Length));

                        var indicator = (termTexts.Count + 1).ToString();

                        topRow.Remove(column.Range.Start.Value, indicator.Length);
                        topRow.Insert(column.Range.Start.Value, indicator);

                        termTexts.Add(indicator, column);
                        continue;
                    }

                    // bar
                    rowText.Remove(column.Range.Start.Value, size);
                    var barText = new StringBuilder("\u2550".PadLeft(size - 3, '\u2550'));

                    barText.Insert(barText.Length / 2, "\u2566"); // T
                    barText.Insert(0, "\u255a"); // corner UR
                    barText.Insert(size - 1, "\u255d"); // corner UL
                    rowText.Insert(column.Range.Start.Value, barText);

                    char DrawVert(char prev)
                    {
                        return prev switch
                        {
                            ' ' => '\u2551',
                            // L: UR -> T: UDR
                            '\u255a' => '\u2560',
                            // L: UL -> T: UDL
                            '\u255d' => '\u2563',
                            // horizontal -
                            '\u2550' => '\u256c',
                            _ => prev
                        };
                    }

                    foreach (var prevRowText in rows)
                    {
                        var prevStart = prevRowText[column.Range.Start.Value];
                        prevRowText.Remove(column.Range.Start.Value, 1);
                        prevRowText.Insert(column.Range.Start.Value, DrawVert(prevStart));

                        prevStart = prevRowText[query.Length + 1 + column.Range.Start.Value];
                        prevRowText.Remove(query.Length + 1 + column.Range.Start.Value, 1);
                        prevRowText.Insert(query.Length + 1 + column.Range.Start.Value, DrawVert(prevStart));

                        var prevEnd = prevRowText[column.Range.End.Value - 1];
                        prevRowText.Remove(column.Range.End.Value - 1, 1);
                        prevRowText.Insert(column.Range.End.Value - 1, DrawVert(prevEnd));

                        prevEnd = prevRowText[query.Length + column.Range.End.Value];

                        prevRowText.Remove(query.Length + column.Range.End.Value, 1);
                        prevRowText.Insert(query.Length + column.Range.End.Value, DrawVert(prevEnd));
                    }

                    // desc
                    var icon = (termTexts.Count + 1).ToString();
                    var desc = $"{icon} [{column.Term.Type}] {column.Name}";

                    if (column.Term.DebugText is not null)
                        desc += $": {column.Term.DebugText.Get()}";


                    if (desc.Length + 2 > size)
                    {
                        desc = $"{icon} [{column.Term.Type}] {column.Name}";
                    }

                    if (desc.Length + 2 > size)
                    {
                        desc = $"{icon} {column.Name}";
                    }

                    if (desc.Length + 2 >= size)
                    {
                        desc = icon;
                    }

                    termTexts.Add(icon, column);

                    var position = query.Length + 1 // line 2
                                                + column.Range.Start.Value // start of the slice
                                                + size / 2 // half of the slices' length : center of the slice
                                   - (desc.Length == 1
                                       ? size % 2 == 0 ? 1 : 0 // don't ask
                                       : desc.Length / 2); // half of the contents length : centers it

                    rowText.Remove(position, desc.Length);
                    rowText.Insert(position, desc);
                }

                rows.Add(rowText);
            }

            if (topRow is not null)
                sb.AppendLine(topRow.ToString());

            var tokenRow = new StringBuilder("".PadLeft(query.Length));

            const char leftChar = '\u250c';
            const char wall = '\u2500';
            const char rightChar = '\u2510';

            var i = 0;
            foreach (var token in tokens)
            {
                var section = token > 2 ? "".PadRight(token - 2, wall) : string.Empty;

                switch (token)
                {
                    case 1:
                        tokenRow.Insert(i, '|');
                        break;
                    case 2:
                        tokenRow.Insert(i, $"{leftChar}{rightChar}");
                        break;
                    default:
                        tokenRow.Insert(i, $"{leftChar}{section}{rightChar}");
                        break;
                }

                i += token;
            }

            sb.AppendLine(tokenRow.ToString());

            sb.AppendLine(query);

            foreach (var row in rows)
            {
                sb.AppendLine(row.ToString());
            }

            if (termTexts.Count > 0)
            {
                sb.AppendLine("Terms:");

                foreach (var (name, value) in termTexts)
                {
                    sb.AppendLine($"  {name}. {value.Term.Type} ({value.Range})");
                    sb.AppendLine($"  - Name: {value.Name}");
                    sb.AppendLine($"  - Position: {value.Term.Position}, Size: {value.Term.Size}");
                    if (value.Term.Metadata is not null)
                        sb.AppendLine($"  - {value.Term.Metadata}");
                    if (value.Term.DebugText is not null)
                        sb.AppendLine($"  - Context: {value.Term.DebugText.Get()}");


                    sb.AppendLine();
                }
            }
        }
        else
        {
            sb.AppendLine(query);
        }

        if (variables.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Variables: ");

            foreach (var (name, value) in variables)
            {
                sb.AppendLine($" - {name} := {value}");
            }
        }

        return sb.ToString();
    }

    private static List<List<QuerySpan>> CreateTermView(LinkedList<QuerySpan> spans)
    {
        var ordered =
            new Queue<QuerySpan>(spans.OrderBy(x => x.Range.End.Value - x.Range.Start.Value)); // order by 'size'
        var result = new List<List<QuerySpan>>();
        var row = new List<QuerySpan>();

        while (ordered.TryDequeue(out var span))
        {
            foreach (var prevRow in result)
            {
                if (prevRow.All(y => !span.Range.Overlaps(y.Range)))
                {
                    prevRow.Add(span);
                    goto end_iter;
                }
            }

            if (row.Count == 0)
            {
                row.Add(span);
                continue;
            }

            if (row.Any(x => x.Range.Overlaps(span.Range)))
            {
                // overlap
                result.Add(row);
                row = [span];
                continue;
            }

            row.Add(span);

            end_iter: ;
        }

        result.Add(row);
        return result;
    }


#if DEBUG
    internal static string QuickView(QueryWriter writer)
    {
        var (query, terms, tokens) = writer.CompileDebug();
        return CreateDebugText(query, new Dictionary<string, object?>(), terms, tokens);
    }
#endif
}
