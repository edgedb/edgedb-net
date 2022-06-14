using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Serializer
{
    public sealed class AttributeNamingStrategy : INamingStrategy
    {
        public string GetName(PropertyInfo property)
        {
            return property.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? property.Name;
        }
    }
}
