using System.Reflection;

namespace EdgeDB;

internal sealed class DefaultNamingStrategy : INamingStrategy
{
    public string Convert(PropertyInfo property)
        => property.Name;

    public string Convert(string name)
        => name;
}
