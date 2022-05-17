using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an error with the provided connection details.
    /// </summary>
    public class InvalidConnectionException : EdgeDBException
    {
        public InvalidConnectionException(string message)
            : base(message, false, false)
        {

        }
    }
}
