namespace EdgeDB;

internal static class Defer
{
    public static Deferrable<T> This<T>(Func<T> value) => new(value);
}

internal sealed class Deferrable<T>
{
    private readonly Func<T>? _getValue;

    private bool _isDeferred;
    private T _value;

    public Deferrable(T value)
    {
        _value = value;
        _isDeferred = false;
        _getValue = null;
    }

    public Deferrable(Func<T> value)
    {
        _value = default!;
        _getValue = value;
        _isDeferred = true;
    }

    public T Get()
    {
        lock (this)
        {
            if (!_isDeferred) return _value;
            _isDeferred = false;
            return _value = _getValue!();
        }
    }

    public static implicit operator Deferrable<T>(T value) => new(value);
    public static implicit operator Deferrable<T>(Func<T> value) => new(value);
}
