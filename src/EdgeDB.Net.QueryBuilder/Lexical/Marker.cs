namespace EdgeDB;

internal sealed class Marker
{
    public bool IsAlive { get; private set; } = true;

    public string Name { get; }
    public MarkerType Type { get; }

    public int Position
    {
        get => _position;
        set => UpdatePosition(value - _position);
    }

    public int Size
    {
        get => _size;
        set => UpdateSize(value - _size);
    }

    public Deferrable<string>? DebugText { get; private set;}

    public IMarkerMetadata? Metadata { get; private set; }

    public LooseLinkedList<Value>.NodeSlice Slice { get; private set; }

    private readonly QueryWriter _writer;

    private int _position;
    private int _size;

    internal Marker(string name, MarkerType type, QueryWriter writer, int size, int position, LooseLinkedList<Value>.NodeSlice slice, Deferrable<string>? debugText, IMarkerMetadata? metadata)
    {
        Name = name;
        Type = type;
        _writer = writer;
        Size = size;
        Position = position;
        Slice = slice;
        DebugText = debugText;
        Metadata = metadata;
    }

    internal int UpdatePosition(int delta, bool updateSlicesPosition = false)
    {
        var result = _position += delta;

        if(updateSlicesPosition) UpdateSliceSize(delta, 0);

        return result;
    }

    internal int UpdateSize(int delta)
    {
        var result = _size += delta;

        UpdateSliceSize(0, delta);

        return result;
    }

    private void UpdateSliceSize(int shift, int delta)
    {
        // null when in an init-like setting
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Slice is null)
            return;

        if (shift != 0)
        {
            var shiftDir = shift > 0;
            var shiftAbs = Math.Abs(shift);

            for (var i = 0; i != shiftAbs; i++)
            {
                Slice.Head = shiftDir ? Slice.Head?.Next : Slice.Head?.Previous;
                Slice.Tail = shiftDir ? Slice.Tail?.Next : Slice.Tail?.Previous;
            }
        }

        if (delta != 0)
        {
            var deltaDir = delta > 0;
            var deltaAbs = Math.Abs(delta);

            for (var i = 0; i != deltaAbs; i++)
            {
                Slice.Tail = deltaDir ? Slice.Tail?.Next : Slice.Tail?.Previous;
            }
        }
    }

    public void Replace(Value value)
    {
        _writer.Replace(Position, Slice, in value);
    }

    public void Remove()
        => _writer.Remove(Position, Slice);

    public void Replace(WriterProxy value)
        => Replace(new Value(value));

    public void Replace(LooseLinkedList<Value>.NodeSlice slice, Range slicePoint)
        => _writer.Replace(Position, Slice, Position..Size, slice, slicePoint);

    public void Kill()
    {
        if (!IsAlive) return;

        IsAlive = false;
        Slice = null!;
        Metadata = null!;
        DebugText = null!;
    }
}
