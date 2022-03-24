using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class ParameterMap : Attribute
    {
        internal readonly long Index;
        internal readonly string Name;

        public ParameterMap(long index, string name)
        {
            Index = index;
            Name = name;
        }
    }
}
