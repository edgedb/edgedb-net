using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Utils.FSharp
{
    internal readonly ref struct FSharpOptionInterop
    {
        private static PropertyInfo? _isSomeProperty;
        private static PropertyInfo? _getValueProperty;

        public object? Value
            => _value;

        public bool HasValue
            => _hasValue;

        private readonly object? _value;
        private readonly bool _hasValue;
        private readonly Type _type;

        private FSharpOptionInterop(object? obj)
        {
            _type = obj?.GetType() ?? typeof(object);

            if (obj is null)
                return;

            if (!(_type.IsFSharpOption() || _type.IsFSharpValueOption()))
                throw new InvalidOperationException($"The provided type {_type} is not an F# option");

            _hasValue = (bool)GetIsSomeProperty(_type).GetValue(obj,
                _isSomeProperty!.GetIndexParameters().Length > 0
                    ? new object?[] { obj }
                    : null
            )!;

            _value = HasValue
                ? GetValueProperty(_type).GetValue(obj,
                    _getValueProperty!.GetIndexParameters().Length > 0
                        ? new object?[] { obj }
                        : null
                    )
                : null;
        }

        public static bool TryGet(object? value, out FSharpOptionInterop option)
        {
            if (value is null)
            {
                option = default;
                return false;
            }

            var type = value.GetType();

            if (type.IsFSharpOption() || type.IsFSharpValueOption())
            {
                option = new(value);
                return true;
            }

            option = default;
            return false;
        }

        private static PropertyInfo GetIsSomeProperty(Type type)
            => _isSomeProperty ??= type.GetProperty("IsSome") ?? throw new MissingMethodException($"Can't find 'IsSome' property on {type}");

        private static PropertyInfo GetValueProperty(Type type)
            => _getValueProperty ??= type.GetProperty("Value") ?? throw new MissingMethodException($"Can't find 'GetValue' property on {type}");
    }
}
