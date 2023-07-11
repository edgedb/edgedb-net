using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class TypeExtensions
    {
        public static bool IsRecord(this Type type)
            => type.GetMethods().Any(m => m.Name == "<Clone>$");

        public static bool IsFSharpType(this Type type)
            => type.Module.Name == "FSharp.Core.dll";

        public static bool IsFSharpOption(this Type type)
            => IsFSharpType(type) && type.Name == "FSharpOption`1";

        public static bool IsFSharpValueOption(this Type type)
            => IsFSharpType(type) && type.Name == "FSharpValueOption`1";

        public static Type GetWrappingType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType()!;
            }

            if (type == typeof(Range))
                return typeof(int);

            if (type.IsGenericType)
            {
                if (type.IsFSharpOption() || type.IsFSharpValueOption())
                {
                    return type.GenericTypeArguments[0];
                }

                var genDef = type.GetGenericTypeDefinition();

                if (genDef == typeof(DataTypes.Range<>))
                {
                    return type.GenericTypeArguments[0];
                }
            }

            Type? iface = null;
            if(
                (type.Name == "IEnumerable`1" ? iface = type : null) is not null ||
                ((iface = type.GetInterfaces().FirstOrDefault(x => x.Name == "IEnumerable`1")) is not null))
            {
                return iface!.GenericTypeArguments[0];
            }

            throw new NotSupportedException($"Cannot find inner type of {type}");
        }
    }
}
