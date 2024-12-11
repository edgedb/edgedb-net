using System.Reflection;

namespace EdgeDB;

internal static class TypeExtensions
{
    public static bool References(this Type type, Type other)
        => References(type, other, true, []);

    private static bool References(Type type, Type other, bool checkInterfaces, HashSet<Type> hasChecked)
    {
        if (!hasChecked.Add(type))
            return false;

        if (type == other)
            return true;

        return type switch
        {
            {IsArray: true} => References(type.GetElementType()!, other, true, hasChecked),
            {IsGenericType: true} => type.GetGenericArguments().Any(x => References(x, other, true, hasChecked)),
            _ => (type.BaseType?.References(other) ?? false) || (checkInterfaces &&
                                                                 type.GetInterfaces().Any(x =>
                                                                     References(x, other, false, hasChecked)))
        };
    }

    public static IEnumerable<PropertyInfo> GetEdgeDBTargetProperties(this Type type, bool excludeId = false)
        => type.GetProperties().Where(x =>
            x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null && !(excludeId && x.Name == "Id" &&
                                                                       (x.PropertyType == typeof(Guid) ||
                                                                        x.PropertyType == typeof(Guid?))));

    public static string GetEdgeDBTypeName(this Type type)
    {
        var attr = type.GetCustomAttribute<EdgeDBTypeAttribute>();
        var name = attr?.Name ?? type.Name;
        return attr != null ? $"{(attr.ModuleName != null ? $"{attr.ModuleName}::" : "default::")}{name}" : name;
    }

    public static string GetEdgeDBPropertyName(this MemberInfo info)
    {
        var att = info.GetCustomAttribute<EdgeDBPropertyAttribute>();

        return
            $"{(att?.IsLinkProperty ?? false ? "@" : "")}{att?.Name ?? (info is PropertyInfo p ? TypeBuilder.SchemaNamingStrategy.Convert(p) : TypeBuilder.SchemaNamingStrategy.Convert(info.Name))}";
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

    public static object? GetMemberValue(this MemberInfo info, object? obj) =>
        info switch
        {
            FieldInfo field => field.GetValue(obj),
            PropertyInfo property => property.GetValue(obj),
            _ => throw new InvalidOperationException("Cannot resolve constant member expression")
        };
}
