using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal sealed class CamelCaseNamingStrategy : INamingStrategy
    {
        public string Convert(MemberInfo member)
            => Convert(member.Name);

        public string Convert(string name)
            => $"{char.ToLowerInvariant(name[0])}{name[1..].Replace("_", string.Empty)}";
    }
}
