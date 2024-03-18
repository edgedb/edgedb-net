using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace EdgeDB;

[DebuggerDisplay("{_value}")]
internal abstract class Union(object value)
{
    private readonly object _value = value;

    public bool Is<T>([MaybeNullWhen(false)] out T result)
    {
        if (_value is T v)
        {
            result = v;
            return true;
        }

        result = default!;
        return false;
    }

    public T As<T>()
    {
        if (_value is not T asValue)
            throw new InvalidCastException($"Value of union is not {typeof(T)}");

        return asValue;
    }
}

internal sealed class Union<T, U> : Union
    where T : notnull
    where U : notnull
{
    public Union(T value) : base(value) { }
    public Union(U value) : base(value) { }

    public static Union<T, U> From(object o, Func<Union<T, U>> fallback)
    {
        return o switch
        {
            T a => new(a),
            U b => new Union<T, U>(b),
            _ => fallback()
        };
    }

    public static implicit operator Union<T, U>(T a) => new(a);
    public static implicit operator Union<T, U>(U b) => new(b);
}

internal sealed class Union<T, U, V> : Union
    where T : notnull
    where U : notnull
    where V : notnull
{
    public Union(T value) : base(value) { }
    public Union(U value) : base(value) { }
    public Union(V value) : base(value) { }

    public static Union<T, U, V> From(object o, Func<Union<T, U, V>> fallback)
    {
        return o switch
        {
            T a => new(a),
            U b => new(b),
            V c => new(c),
            _ => fallback()
        };
    }

    public static implicit operator Union<T, U, V>(T a) => new(a);
    public static implicit operator Union<T, U, V>(U b) => new(b);
    public static implicit operator Union<T, U, V>(V c) => new(c);
}
