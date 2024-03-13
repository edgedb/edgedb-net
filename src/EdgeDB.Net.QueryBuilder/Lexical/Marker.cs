namespace EdgeDB;

internal sealed class Marker
{
    public string Name { get; }
    public MarkerType Type { get; }

    public int Position { get; private set; }

    public int Size { get; }

    public Deferrable<string>? DebugText { get; }

    public IMarkerMetadata? Metadata { get; }

    public LooseLinkedList<Value>.Node Start { get; set; }

    private readonly QueryWriter _writer;

    internal Marker(string name, MarkerType type, QueryWriter writer, int size, int position, LooseLinkedList<Value>.Node start, Deferrable<string>? debugText, IMarkerMetadata? metadata)
    {
        Name = name;
        Type = type;
        _writer = writer;
        Size = size;
        Position = position;
        Start = start;
        DebugText = debugText;
        Metadata = metadata;
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

    public void Remove()
        => _writer.Remove(Position, Start, Size);

    public void Replace(WriterProxy value)
        => Replace(new Value(value));
}
