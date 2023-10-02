using System.Reflection;

namespace EdgeDB;

internal sealed class AttributeNamingStrategy : INamingStrategy
{
    public string Convert(PropertyInfo property) =>
        property.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? property.Name;

    public string Convert(string name) => name;
}
