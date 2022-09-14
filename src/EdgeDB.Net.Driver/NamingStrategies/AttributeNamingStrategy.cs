using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public sealed class AttributeNamingStrategy : INamingStrategy
    {
        public string Convert(PropertyInfo property)
        {
            return property.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? property.Name;
        }

        public string Convert(string name) => name;
    }
}
