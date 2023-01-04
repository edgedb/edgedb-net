using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class TypeExtensions
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
    }
}
