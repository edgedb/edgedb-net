using System.Reflection;

namespace EdgeDB;

internal sealed class CamelCaseNamingStrategy : INamingStrategy
{
    public string Convert(PropertyInfo property)
        => Convert(property.Name);

    public string Convert(string name)
        => $"{char.ToLowerInvariant(name[0])}{name[1..].Replace("_", string.Empty)}";
}
