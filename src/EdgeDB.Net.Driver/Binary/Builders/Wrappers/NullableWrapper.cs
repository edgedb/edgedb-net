using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Binary.Builders.Wrappers
{
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
                _constructor ??= target.GetConstructor(new Type[] { value.GetType() })
                    ?? throw new EdgeDBException($"Failed to find constructor for {target}")
            ).Invoke(new object?[] { value });
        }
    }
}
