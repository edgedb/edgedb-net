namespace EdgeDB;

internal sealed class Marker
{
    public MarkerType Type { get; }
    public int Position { get; private set; }
    public int Size { get; }

    /// <summary>
    ///     Gets the closest parent to this marker
    /// </summary>
    public Marker? Parent
    {
        get
        {
            Marker? bestMatch = null;

            foreach (var marker in _writer.Labels.Values.SelectMany(x => x))
            {
                if(marker == this)
                    continue;

                if (marker.Position <= Position && marker.Size >= Size)
                {
                    if (bestMatch is null)
                    {
                        bestMatch = marker;
                        continue;
                    }

                    if (marker.Position > bestMatch.Position || marker.Size < bestMatch.Size)
                        bestMatch = marker;
                }
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
            return _writer.Labels.Values.SelectMany(x => x).Where(x => x != this && x.Position <= Position && x.Size >= Size)
                .OrderBy(x => Position - x.Position + x.Size);
        }
    }

    private readonly QueryStringWriter _writer;

    internal Marker(MarkerType type, QueryStringWriter writer, int size, int position)
    {
        Type = type;
        _writer = writer;
        Size = size;
        Position = position;
    }

    internal void Update(int delta)
    {
        Position += delta;
    }

    public void Replace(Value value)
    {
        _writer
            .Remove(Position, Size)
            .Insert(Position, value);
    }

    public void Replace(WriterProxy value)
        => Replace(new Value(value));
}
