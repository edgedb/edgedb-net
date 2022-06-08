using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception thrown when no type converter could be found.
    /// </summary>
    public class NoTypeConverter : EdgeDBException
    {
        public NoTypeConverter(Type target, Type source)
            : base($"Could not convert {source.Name} to {target.Name}", false, false)
        {

        }
    }
}
