using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Serializer
{
    public sealed class CamelCaseNamingStrategy : INamingStrategy
    {
        public string GetName(PropertyInfo property)
        {
            var name = property.Name;
            return $"{char.ToLowerInvariant(name[0])}{name[1..].Replace("_", string.Empty)}";
        }
    }
}
