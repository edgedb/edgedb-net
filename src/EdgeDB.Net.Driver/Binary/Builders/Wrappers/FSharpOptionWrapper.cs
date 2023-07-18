using EdgeDB.Utils.FSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Builders.Wrappers
{
    internal sealed class FSharpOptionWrapper : IWrapper
    {
        private static ConstructorInfo? _valueOptionConstructor;
        private static ConstructorInfo? _referenceOptionConstructor;

        public Type GetInnerType(Type wrapperType)
            => wrapperType.GenericTypeArguments[0];

        public bool IsWrapping(Type t)
            => t.IsFSharpOption() || t.IsFSharpValueOption();

        public object? Wrap(Type target, object? value)
        {
            if (target.IsFSharpValueOption())
                return WrapValueOption(target, value);
            else if (target.IsFSharpOption())
                return WrapReferenceOption(target, value);
            else
                throw new NotSupportedException($"Unsupported wrapping type: {target}");
        }

        private static object? WrapReferenceOption(Type target, object? value)
        {
            if (value is null)
                return null;

            return (
                _referenceOptionConstructor ??= target.GetConstructor(new Type[] { value.GetType() })
                    ?? throw new EdgeDBException($"Failed to find constructor for {target}")
            ).Invoke(new object?[] { value });
        }

        private static object? WrapValueOption(Type target, object? value)
        {
            if (value is null)
                return ReflectionUtils.GetDefault(target);

            return (
                _valueOptionConstructor ??= target.GetConstructor(new Type[] { value.GetType() })
                    ?? throw new EdgeDBException($"Failed to find constructor for {target}")
            ).Invoke(new object?[] { value });
        }
    }
}
