using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal sealed class DefaultNamingStrategy : INamingStrategy
    {
        public string Convert(MemberInfo member)
            => member.Name;
        public string Convert(string name)
            => name;
    }
}
