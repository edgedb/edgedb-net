using System.Diagnostics;

namespace EdgeDB;

internal sealed class Term
{
    public bool IsAlive { get; private set; } = true;

    public string Name { get; }
    public TermType Type { get; }

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

    public Range Range => Position..Size;

    public Deferrable<string>? DebugText { get; private set;}

    public ITermMetadata? Metadata { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public LooseLinkedList<Token>.NodeSlice Slice
    {
        get
        {
            if (_sliceVersion != _version)
                RecalculateSlice();

            return _slice;
        }
    }

    private readonly QueryWriter _writer;

    private int _position;
    private int _size;
    private int _sliceVersion;
    private int _version;
    private LooseLinkedList<Token>.NodeSlice _slice;

    internal Term(string name, TermType type, QueryWriter writer, int size, int position, LooseLinkedList<Token>.NodeSlice slice, Deferrable<string>? debugText, ITermMetadata? metadata)
    {
        Name = name;
        Type = type;
        _writer = writer;
        Size = size;
        Position = position;
        _slice = slice;
        DebugText = debugText;
        Metadata = metadata;
    }

    public bool IsChildOf(Term term)
        => term.Position <= Position && term.Size >= Size;

    public int SizeDistance(Term term)
    {
        var a = Position - term.Position;
        var b = term.Size;

        return a + b;
    }

    internal int UpdatePosition(int delta)
    {
        if (delta != 0)
            _version++;

        return _position += delta;
    }

    internal int UpdateSize(int delta)
    {
        if (delta != 0)
            _version++;

        return _size += delta;
    }

    private void RecalculateSlice()
    {
        if (_version == _sliceVersion)
            return;

        if (_slice.Head is null || !_slice.Head.IsAlive)
        {
            _slice = LooseLinkedList<Token>.NodeSlice.Empty;
            _sliceVersion = _version;
            IsAlive = false;
            return;
        }

        if (Size <= 0)
        {
            _sliceVersion = _version;
            _slice = LooseLinkedList<Token>.NodeSlice.Empty;
            return;
        }

        _slice = _writer.Tokens.Slice(_slice.Head, Size);
        _sliceVersion = _version;
    }

    public void Replace(Token token)
    {
        _writer.Move(Position, Slice, in token);
    }

    public void Remove()
        => _writer.Remove(Position, Slice);

    public void Replace(WriterProxy value)
        => Replace(new Token(value));

    public void Move(LooseLinkedList<Token>.NodeSlice slice, Range slicePoint)
        => _writer.Move(Slice, Range, slice, slicePoint);

    public void Kill()
    {
        if (!IsAlive) return;

        IsAlive = false;
        _slice = null!;
        Metadata = null!;
        DebugText = null!;
    }
}
