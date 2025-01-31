using System.Reflection;

namespace Gel;

internal sealed class AttributeNamingStrategy : INamingStrategy
{
    public string Convert(PropertyInfo property) =>
        property.GetCustomAttribute<GelPropertyAttribute>()?.Name ?? property.Name;

    public string Convert(string name) => name;
}
