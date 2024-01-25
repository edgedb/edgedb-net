namespace EdgeDB.Utils;

internal sealed class Ref<T>(T value)
where T : class
{
    public T Value { get; set; } = value;

    public static implicit operator Ref<T>(T value) => new(value);
    public static implicit operator T(Ref<T> reference) => reference.Value;
}
