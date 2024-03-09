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

    private static string CreateDebugText(string query, Dictionary<string, object?> variables,
        LinkedList<QuerySpan> markers)
    {
        var sb = new StringBuilder();

        if (markers.Count > 0)
        {
            StringBuilder? topRow = null;

            var view = CreateMarkerView(markers);
            var markerTexts = new Dictionary<string, QuerySpan>();
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

                        var indicator = (markerTexts.Count + 1).ToString();

                        topRow.Remove(column.Range.Start.Value, indicator.Length);
                        topRow.Insert(column.Range.Start.Value, indicator);

                        markerTexts.Add(indicator, column);
                        continue;
                    }

                    // bar
                    rowText.Remove(column.Range.Start.Value, size);
                    var barText = new StringBuilder($"\u2550".PadLeft(size - 3, '\u2550'));

                    barText.Insert(barText.Length / 2, "\u2566"); // T
                    barText.Insert(0, "\u255a"); // corner UR
                    barText.Insert(size - 1, "\u255d"); // corner UL
                    rowText.Insert(column.Range.Start.Value, barText);

                    foreach (var prevRowText in rows)
                    {
                        var prevStart = prevRowText[column.Range.Start.Value];

                        if (prevStart is ' ' or '\u255a')
                        {
                            prevRowText.Remove(column.Range.Start.Value, 1);
                            prevRowText.Insert(column.Range.Start.Value, prevStart == '\u255a' ? '\u2560' : '\u2551');
                        }

                        prevStart = prevRowText[query.Length + 1 + column.Range.Start.Value];

                        if (prevStart is ' ' or '\u255a')
                        {
                            prevRowText.Remove(query.Length + 1 + column.Range.Start.Value, 1);
                            prevRowText.Insert(query.Length + 1 + column.Range.Start.Value, prevStart == '\u255a' ? '\u2560' : '\u2551');
                        }

                        var prevEnd = prevRowText[column.Range.End.Value - 1];

                        if (prevEnd is ' ' or '\u255d')
                        {
                            prevRowText.Remove(column.Range.End.Value - 1, 1);
                            prevRowText.Insert(column.Range.End.Value - 1, prevEnd == '\u255d' ? '\u2563' : '\u2551');
                        }

                        prevEnd = prevRowText[query.Length + column.Range.End.Value];

                        if (prevEnd is ' ' or '\u255d')
                        {
                            prevRowText.Remove(query.Length + column.Range.End.Value, 1);
                            prevRowText.Insert(query.Length + column.Range.End.Value,  prevEnd == '\u255d' ? '\u2563' : '\u2551');
                        }
                    }

                    // desc
                    var icon = (markerTexts.Count + 1).ToString();
                    var desc = $"{icon} [{column.Marker.Type}] {column.Name}";

                    if (column.Marker.DebugText is not null)
                        desc += $": {column.Marker.DebugText.Get()}";


                    if (desc.Length - 3 > size)
                    {
                        desc = $"{icon} [{column.Marker.Type}] {column.Name}";
                    }

                    if (desc.Length - 3 > size)
                    {
                        desc = $"{icon} {column.Name}";
                    }

                    if (desc.Length - 3 >= size)
                    {
                        desc = icon;
                    }

                    markerTexts.Add(icon, column);

                    var position = query.Length + 1  // line 2
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

            sb.AppendLine(query);

            foreach (var row in rows)
            {
                sb.AppendLine(row.ToString());
            }

            if (markerTexts.Count > 0)
            {
                sb.AppendLine("Markers:");

                var markerTypePadding = Enum.GetValues<MarkerType>().Max(x => Enum.GetName(x)!.Length) + 2;
                foreach (var (name, value) in markerTexts)
                {
                    var desc = $"{$"[{value.Marker.Type}]".PadRight(markerTypePadding)} {value.Name}";
                    if (value.Marker.DebugText is not null)
                        desc += $": {value.Marker.DebugText.Get()}";

                    sb.AppendLine($" - {name.PadRight(markerTexts.Count.ToString().Length)} {$"({value.Range})".PadRight(query.Length.ToString().Length*2+4)} {desc}");
                }
            }
        }
        else
        {
            sb.AppendLine(query);
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
