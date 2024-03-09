namespace EdgeDB;

internal sealed class Marker
{
    public MarkerType Type { get; }
    public int Position { get; private set; }
    public int Size { get; }
    public Deferrable<string>? DebugText { get; }

    public LooseLinkedList<Value>.Node Start { get; set; }

    /// <summary>
    ///     Gets the closest parent to this marker
    /// </summary>
    public Marker? Parent
    {
        get
        {
            Marker? bestMatch = null;

            foreach (var marker in _writer.Markers.Values.SelectMany(x => x))
            {
                if(marker == this)
                    continue;

                if (marker.Position > Position || marker.Size < Size) continue;
                if (bestMatch is null)
                {
                    bestMatch = marker;
                    continue;
                }

                if (marker.Position > bestMatch.Position || marker.Size < bestMatch.Size)
                    bestMatch = marker;
            }

            return bestMatch;
        }
    }

    /// <summary>
    ///     Gets a collection of parents of this marker, with the first being the closest parent; and so on.
    /// </summary>
    public IOrderedEnumerable<Marker> Parents
    {
        get
        {
            return _writer.Markers.Values.SelectMany(x => x).Where(x => x != this && x.Position <= Position && x.Size >= Size)
                .OrderBy(x => Position - x.Position + x.Size);
        }
    }

    private readonly QueryWriter _writer;

    internal Marker(MarkerType type, QueryWriter writer, int size, int position, LooseLinkedList<Value>.Node start, Deferrable<string>? debugText)
    {
        Type = type;
        _writer = writer;
        Size = size;
        Position = position;
        Start = start;
        DebugText = debugText;
    }

    internal void Update(int delta)
    {
        Position += delta;
    }

    public void Replace(Value value)
    {
        _writer
            .Replace(Start, Position, Size, value);
    }

    public void Replace(WriterProxy value)
        => Replace(new Value(value));
}
