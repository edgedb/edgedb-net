namespace EdgeDB.Binary.Builders.Wrappers;

internal sealed class FSharpOptionWrapper : IWrapper
{
    public Type GetInnerType(Type wrapperType)
        => wrapperType.GenericTypeArguments[0];

    public bool IsWrapping(Type t)
        => t.IsFSharpOption() || t.IsFSharpValueOption();

    public object? Wrap(Type target, object? value)
    {
        if (target.IsFSharpValueOption())
            return WrapValueOption(target, value);
        if (target.IsFSharpOption())
            return WrapReferenceOption(target, value);
        throw new NotSupportedException($"Unsupported wrapping type: {target}");
    }

    private static object? WrapReferenceOption(Type target, object? value)
    {
        if (value is null)
            return null;

        return (
            target.GetConstructor(new[] {value.GetType()})
            ?? throw new EdgeDBException($"Failed to find constructor for {target}")
        ).Invoke(new[] {value});
    }

    private static object? WrapValueOption(Type target, object? value)
    {
        if (value is null)
            return ReflectionUtils.GetDefault(target);

        return (
            target.GetConstructor(new[] {value.GetType()})
            ?? throw new EdgeDBException($"Failed to find constructor for {target}")
        ).Invoke(new[] {value});
    }
}
