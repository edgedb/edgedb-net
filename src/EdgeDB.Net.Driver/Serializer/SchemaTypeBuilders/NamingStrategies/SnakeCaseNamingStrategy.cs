using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.Serializer
{
    public sealed class SnakeCaseNamingStrategy : INamingStrategy
    {
        public string GetName(PropertyInfo property)
        {
            return Regex.Replace(property.Name, "(?<!^)[A-Z]", x => $"_{x.Value}").ToLower();
        }
    }
}
