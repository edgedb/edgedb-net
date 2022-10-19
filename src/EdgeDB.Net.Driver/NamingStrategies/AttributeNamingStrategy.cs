using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal sealed class AttributeNamingStrategy : INamingStrategy
    {
        public string Convert(MemberInfo member)
        {
            return member.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? member.Name;
        }

        public string Convert(string name) => name;
    }
}
