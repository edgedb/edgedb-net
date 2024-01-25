namespace EdgeDB;

internal sealed class Marker
{
    public MarkerType Type { get; }
    public int Position { get; private set; }
    public int Size { get; }

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
