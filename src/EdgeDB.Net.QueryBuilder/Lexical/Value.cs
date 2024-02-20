using System.Diagnostics;
using System.Text;

namespace EdgeDB;

[DebuggerDisplay("{DebugDisplay()}")]
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

    public static Value Of(WriterProxy proxy) => new(proxy);

    public void WriteTo(QueryWriter queryWriter, StringBuilder writer, ref LooseLinkedList<Value>.Node self, int index)
    {
        if (_callback is not null)
        {
            using var positional = queryWriter.CreatePositionalWriter(index, ref self);
            _callback(positional.Writer);
        }
        else
        {
            if (_str is not null)
                writer.Append(_str);
            else if (_ch is not null)
                writer.Append(_ch.Value);
            else
                writer.Append(_value);
        }
    }

    private string DebugDisplay()
    {
        if (_callback is not null)
            return $"callback<{_callback}>";

        if (_str is not null)
            return $"str \"{_str}\"";

        return _ch is not null ? $"char \'{_ch}\'" : $"value {_value}";
    }

    public static implicit operator Value(string? value) => new(value);
    public static implicit operator Value(char value) => new(value);
    public static implicit operator Value(WriterProxy callback) => new(callback);
    public static implicit operator Value(int v) => new(v.ToString());
    public static implicit operator Value(long v) => new(v.ToString());
}
