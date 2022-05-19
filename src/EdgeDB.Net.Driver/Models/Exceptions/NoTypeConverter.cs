using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class NoTypeConverter : EdgeDBException
    {
        public NoTypeConverter(Type target, Type source)
            : base($"Could not convert {source.Name} to {target.Name}", false, false)
        {

        }
    }
}
