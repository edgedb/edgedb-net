using System.Reflection;

namespace EdgeDB;

internal sealed class PascalNamingStrategy : INamingStrategy
{
    public string Convert(PropertyInfo property)
        => Convert(property.Name);

    public string Convert(string name)
    {
        var sample = string.Join("",
            name.Select(c => char.IsLetterOrDigit(c) ? c.ToString().ToLower() : "_").ToArray());

        var arr = sample
            .Split(new[] {'_'}, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => $"{s[..1].ToUpper()}{s[1..]}");

        return string.Join("", arr);
    }
}
