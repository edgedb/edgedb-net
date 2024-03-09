using System.Diagnostics;
using System.Text;

namespace EdgeDB.Compiled;

[DebuggerDisplay("{DebugView}")]
public sealed class DebugCompiledQuery : CompiledQuery
{
    public string DebugView { get; }

    internal DebugCompiledQuery(string query, Dictionary<string, object?> variables, LinkedList<QuerySpan> markers)
        : base(query, variables)
    {
        DebugView = CreateDebugText(query, variables, markers);
    }

    private static string NumberCircle(int i)
    {
        if (i <= 50)
        {
            return i == 0 ? "\u24ea" : ((char)('\u2460' + i - 1)).ToString();
        }

        return i.ToString();
    }

    private static string CreateDebugText(string query, Dictionary<string, object?> variables,
        LinkedList<QuerySpan> markers)
    {
        var sb = new StringBuilder();

        sb.AppendLine(query);

        if (markers.Count > 0)
        {
            var view = CreateMarkerView(markers);
            var markerTexts = new Dictionary<string, string>();
            var rows = new List<StringBuilder>();

            foreach (var row in view)
            {
                var rowText = new StringBuilder($"{"".PadLeft(query.Length)}\n{"".PadLeft(query.Length)}");


                foreach (var column in row)
                {
                    var size = column.Range.End.Value - column.Range.Start.Value;

                    // bar
                    rowText.Remove(column.Range.Start.Value, size);
                    var barText = new StringBuilder($"\u2550".PadLeft(size - 3, '\u2550'));
                    barText.Insert(barText.Length / 2, "\u2566"); // T
                    barText.Insert(0, "\u255a"); // corner UR
                    barText.Insert(size - 1, "\u255d"); // corner UL
                    rowText.Insert(column.Range.Start.Value, barText);

                    foreach (var prevRowText in rows)
                    {
                        prevRowText.Remove(column.Range.Start.Value, 1);
                        prevRowText.Insert(column.Range.Start.Value, '\u2551');
                        prevRowText.Remove(query.Length + 1 + column.Range.Start.Value, 1);
                        prevRowText.Insert(query.Length + 1 + column.Range.Start.Value, '\u2551');

                        prevRowText.Remove(column.Range.End.Value - 1, 1);
                        prevRowText.Insert(column.Range.End.Value - 1, '\u2551');
                        prevRowText.Remove(query.Length + column.Range.End.Value, 1);
                        prevRowText.Insert(query.Length + column.Range.End.Value, '\u2551');
                    }

                    // desc
                    var desc = $"{column.Marker.Type}: {column.Name}";
                    string descriptionText;

                    if (desc.Length > size)
                    {
                        var icon = NumberCircle(markerTexts.Count + 1);
                        markerTexts.Add(icon, desc);
                        descriptionText = icon;
                    }
                    else
                    {
                        descriptionText = desc;
                    }

                    var position = query.Length + 1  // line 2
                        + column.Range.Start.Value // start of the slice
                        + size / 2 // half of the slices' length : center of the slice
                        - (descriptionText.Length == 1 ? 1 : descriptionText.Length / 2); // half of the contents length : centers it

                    rowText.Remove(position, descriptionText.Length);
                    rowText.Insert(position, descriptionText);
                }

                rows.Add(rowText);

            }

            foreach (var row in rows)
            {
                sb.AppendLine(row.ToString());
            }

            if (markerTexts.Count > 0)
            {
                sb.AppendLine("Markers:");

                foreach (var (name, value) in markerTexts)
                {
                    sb.AppendLine($" - {name}: {value}");
                }
            }
        }
        else
        {
            sb.AppendLine();
        }

        if (variables.Count > 0)
        {
            sb.AppendLine("Variables: ");

            foreach (var (name, value) in variables)
            {
                sb.AppendLine($" - {name} := {value}");
            }
        }

        return sb.ToString();
    }

    private static List<List<QuerySpan>> CreateMarkerView(LinkedList<QuerySpan> spans)
    {
        var ordered = new Queue<QuerySpan>(spans.OrderBy(x => x.Range.End.Value - x.Range.Start.Value)); // order by 'size'
        var result = new List<List<QuerySpan>>();
        var row = new List<QuerySpan>();

        while (ordered.TryDequeue(out var span))
        {
            var head = row.LastOrDefault();
            if (head is null)
            {
                row.Add(span);
                continue;
            }

            if (head.Range.End.Value >= span.Range.Start.Value)
            {
                // overlap
                result.Add(row);
                row = [span];
                continue;
            }

            row.Add(span);
        }

        result.Add(row);
        return result;
    }
}
