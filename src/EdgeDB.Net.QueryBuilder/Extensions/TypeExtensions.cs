using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class QBTypeExtensions
    {
        public static bool IsAnonymousType(this Type type)
        {
            return
                type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0 &&
                type.FullName!.Contains("AnonymousType");
        }

        public static IEnumerable<PropertyInfo> GetEdgeDBTargetProperties(this Type type, bool excludeId = false)
            => type.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null && !(excludeId && x.Name == "Id" && (x.PropertyType == typeof(Guid) || x.PropertyType == typeof(Guid?))));

        public static string GetEdgeDBTypeName(this Type type)
        {
            var attr = type.GetCustomAttribute<EdgeDBTypeAttribute>();
            var name = attr?.Name ?? type.Name;
            return attr != null ? $"{(attr.ModuleName != null ? $"{attr.ModuleName}::" : "default::")}{name}" : name;
        }
        public static string GetEdgeDBPropertyName(this MemberInfo info)
        {
            var att = info.GetCustomAttribute<EdgeDBPropertyAttribute>();

            return $"{((att?.IsLinkProperty ?? false) ? "@" : "")}{att?.Name ?? TypeBuilder.SchemaNamingStrategy.Convert(info)}";
        }

        public static Type GetMemberType(this MemberInfo info)
        {
            switch (info)
            {
                case PropertyInfo propertyInfo:
                    return propertyInfo.PropertyType;
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
                default:
                    throw new NotSupportedException();
            }
        }

        public static object? GetMemberValue(this MemberInfo info, object? obj)
        {
            return info switch
            {
                FieldInfo field => field.GetValue(obj),
                PropertyInfo property => property.GetValue(obj),
                _ => throw new InvalidOperationException("Cannot resolve constant member expression")
            };
        }

        public static bool IsUnsignedNumber(this Type? type)
        {
            return type == typeof(byte) ||
                type == typeof(ushort) ||
                type == typeof(uint) ||
                type == typeof(ulong);
        }

        public static bool IsSignedNumber(this Type? type)
        {
            return type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(double) ||
                type == typeof(float);
        }

        public static int GetSizeOfMarshalledType(this Type type)
        {
            var method = typeof(Marshal).GetMethod("SizeOf", 1, Type.EmptyTypes)!;

            return (int)method.MakeGenericMethod(type).Invoke(null, null)!;
        }

        public static bool IsNumericType(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static unsafe object ConvertToTargetNumber(this Type source, object sourceValue, Type target)
        {
            if (source == target)
                return sourceValue;

            if (source.IsAssignableTo(target))
                return Convert.ChangeType(sourceValue, target);

            var method = typeof(QBTypeExtensions).GetMethods()
                .First(x => x.Name == "ConvertToTargetNumber" && x.GetGenericArguments().Length == 2)!;

            return method.MakeGenericMethod(new Type[] { source, target }).Invoke(null, new object[] { sourceValue })!;
        }

        public static unsafe TTo ConvertToTargetNumber<TFrom, TTo>(TFrom value)
            => Unsafe.As<TFrom, TTo>(ref value);
    }
}
