using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EdgeDBType : Attribute
    {
        internal readonly string? Name;

        public EdgeDBType(string name)
        {
            Name = name;
        }

        public EdgeDBType() { }
    }
}
