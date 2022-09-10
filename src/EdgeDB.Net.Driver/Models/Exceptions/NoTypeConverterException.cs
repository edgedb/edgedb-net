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
    public class NoTypeConverterException : EdgeDBException
    {
        public NoTypeConverterException(Type target, Type source)
            : base($"Could not convert {source.Name} to {target.Name}", false, false)
        {

        }

        public NoTypeConverterException(Type target, Type source, Exception inner)
            : base($"Could not convert {source.Name} to {target.Name}", inner, false, false)
        {

        }

        public NoTypeConverterException(string message, Exception? inner = null)
            : base(message, inner)
        {

        }
    }
}
