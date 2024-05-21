using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace EdgeDB;

[DebuggerDisplay("{DebugDisplay()}")]
internal readonly struct Value : IEquatable<Value>
{
    [MemberNotNullWhen(false, nameof(Callback))]
    public bool IsScalar
        => Callback is null;

    public static readonly Value Empty = new((object?)null);

    public readonly WriterProxy? Callback;
    public readonly object? RawValue;
    public readonly string? StringValue;
    public readonly char? CharValue;

    public Value(string? stringValue)
    {
        StringValue = stringValue;
    }

    public Value(char charValue)
    {
        CharValue = charValue;
    }

    public Value(WriterProxy? callback)
    {
        Callback = callback;
    }

    public Value(object? rawValue)
    {
        RawValue = rawValue;
    }

    public static Value Of(WriterProxy proxy) => new(proxy);

    public bool TryProxy(
        QueryWriter writer,
        out LooseLinkedList<Value>.Node? first,
        out LooseLinkedList<Value>.Node? last)
    {
        if (IsScalar)
        {
            first = null;
            last = null;
            return false;
        }

        using var nodeObserver = new RangeNodeObserver(writer);
        Callback(writer);

        first = nodeObserver.First;
        last = nodeObserver.Last;
        return true;
    }

    public void WriteTo(StringBuilder writer)
    {
        if (Callback is not null)
        {
            throw new InvalidOperationException("Cannot compile callbacks");
        }

        if (StringValue is not null)
            writer.Append(StringValue);
        else if (CharValue is not null)
            writer.Append(CharValue.Value);
        else
            writer.Append(RawValue);
    }

    private string DebugDisplay()
    {
        if (Callback is not null)
            return $"callback<{Callback}>";

        if (StringValue is not null)
            return $"str \"{StringValue}\"";

        return CharValue is not null ? $"char \'{CharValue}\'" : RawValue is null ? "null" : $"value {RawValue}";
    }

    public override string ToString()
    {
        if (Callback is not null)
            return "<callback>";
        if (StringValue is not null)
            return "<str>";
        if (CharValue.HasValue)
            return "<char>";

        return "<object>";
    }

    public static implicit operator Value(string? value) => new(value);
    public static implicit operator Value(char value) => new(value);
    public static implicit operator Value(WriterProxy callback) => new(callback);
    public static implicit operator Value(int v) => new(v.ToString());
    public static implicit operator Value(long v) => new(v.ToString());

    public bool Equals(Value other) => Equals(Callback, other.Callback) && Equals(RawValue, other.RawValue) && StringValue == other.StringValue && CharValue == other.CharValue;

    public override bool Equals(object? obj) => obj is Value other && Equals(other);

    public bool Equals(string str) => IsScalar && (StringValue == str || (RawValue?.Equals(str) ?? false));

    public override int GetHashCode() => HashCode.Combine(Callback, RawValue, StringValue, CharValue);

    public static bool operator ==(Value left, Value right) => left.Equals(right);

    public static bool operator !=(Value left, Value right) => !left.Equals(right);
}
