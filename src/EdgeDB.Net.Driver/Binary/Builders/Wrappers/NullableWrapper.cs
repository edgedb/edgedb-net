using System.Reflection;

namespace EdgeDB.Binary.Builders.Wrappers;

internal sealed class NullableWrapper : IWrapper
{
    private ConstructorInfo? _constructor;

    public Type GetInnerType(Type wrapperType)
        => wrapperType.GenericTypeArguments[0];

    public bool IsWrapping(Type t)
        => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);

    public object? Wrap(Type target, object? value)
    {
        if (value is null)
            return ReflectionUtils.GetDefault(target);

        return (
            _constructor ??= target.GetConstructor(new[] {value.GetType()})
                             ?? throw new EdgeDBException($"Failed to find constructor for {target}")
        ).Invoke(new[] {value});
    }
}
