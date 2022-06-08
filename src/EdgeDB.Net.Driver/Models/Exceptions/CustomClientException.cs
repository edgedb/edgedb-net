using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a generic error with custom clients.
    /// </summary>
    public sealed class CustomClientException : EdgeDBException
    {
        public CustomClientException(string message)
            : base(message, false, false)
        {

        }
    }
}
