using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB
{
    public sealed class SnakeCaseNamingStrategy : INamingStrategy
    {
        public string Convert(PropertyInfo property)
            => Convert(property.Name);

        public string Convert(string name)
            => Regex.Replace(name, "(?<!^)[A-Z]", x => $"_{x.Value}").ToLower();
    }
}
