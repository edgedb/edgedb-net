namespace EdgeDB;

internal readonly struct Value
{
    public static readonly Value Empty = new((object?)null);

    private readonly WriterProxy? _callback;
    private readonly object? _value;
    private readonly string? _str;
    private readonly char? _ch;

    public Value(string? str)
    {
        _str = str;
    }

    public Value(char ch)
    {
        _ch = ch;
    }

    public Value(WriterProxy? callback)
    {
        _callback = callback;
    }

    public Value(object? value)
    {
        _value = value;
    }

    public void WriteTo(QueryStringWriter writer)
    {
        if (_callback is not null)
            _callback(writer);
        else
        {
            if (_value is QueryStringWriter qsw)
                writer.Append(qsw);
            else if (_str is not null)
                writer.Append(_str);
            else if (_ch is not null)
                writer.Append(_ch.Value);
            else
                writer.Append(_value);
        }
    }

    public void WriteAt(QueryStringWriter writer, int index)
    {
        if (_callback is not null)
            _callback(writer.GetPositionalWriter(index));
        else
        {
            if (_value is QueryStringWriter qsw)
                writer.Insert(index, qsw);
            else if (_str is not null)
                writer.Insert(index, _str);
            else if (_ch is not null)
                writer.Insert(index, _ch.Value);
            else
                writer.Insert(index, _value);
        }
    }


    public static implicit operator Value(string? value) => new(value);
    public static implicit operator Value(char value) => new(value);
    public static implicit operator Value(QueryStringWriter writer) => new(writer);
    public static implicit operator Value(WriterProxy callback) => new(callback);
}
