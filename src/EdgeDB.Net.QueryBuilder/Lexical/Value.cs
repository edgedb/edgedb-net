using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace EdgeDB;

//[DebuggerDisplay("{DebugDisplay()}")]
internal readonly struct Value
{
    [MemberNotNullWhen(false, nameof(_callback))]
    public bool IsScalar
        => _callback is null;

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

    public ref LooseLinkedList<Value>.Node Proxy(QueryWriter writer, out bool success)
    {
        if (IsScalar)
        {
            success = false;
            return ref Unsafe.NullRef<LooseLinkedList<Value>.Node>();
        }

        using var nodeObserver = new LastNodeObserver(writer);
        _callback(writer);

        if (!nodeObserver.HasValue)
            throw new InvalidOperationException("Provided proxy wrote no value");

        success = true;
        return ref nodeObserver.Value;
    }

    public void WriteTo(StringBuilder writer)
    {
        if (_callback is not null)
        {
            throw new InvalidOperationException("Cannot compile callbacks");
        }

        if (_str is not null)
            writer.Append(_str);
        else if (_ch is not null)
            writer.Append(_ch.Value);
        else
            writer.Append(_value);
    }

    private string DebugDisplay()
    {
        if (_callback is not null)
            return $"callback<{_callback}>";

        if (_str is not null)
            return $"str \"{_str}\"";

        return _ch is not null ? $"char \'{_ch}\'" : _value is null ? "null" : $"value {_value}";
    }

    public static implicit operator Value(string? value) => new(value);
    public static implicit operator Value(char value) => new(value);
    public static implicit operator Value(WriterProxy callback) => new(callback);
    public static implicit operator Value(int v) => new(v.ToString());
    public static implicit operator Value(long v) => new(v.ToString());
}
