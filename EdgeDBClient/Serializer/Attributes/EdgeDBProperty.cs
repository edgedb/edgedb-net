using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EdgeDBProperty : Attribute
    {
        internal readonly string? Name;

        public EdgeDBProperty(string? propertyName = null)
        {
            Name = propertyName;
        }
    }
}
