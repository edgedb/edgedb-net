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
        public string Convert(PropertyInfo property)
            => property.Name;
        public string Convert(string name)
            => name;
    }
}
