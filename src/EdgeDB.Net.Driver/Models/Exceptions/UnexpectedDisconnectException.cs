using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an exception that was caused by an unexpected disconnection from EdgeDB.
    /// </summary>
    public class UnexpectedDisconnectException : EdgeDBException
    {
        internal UnexpectedDisconnectException()
            : base("The connection was unexpectedly closed", false, false)
        {
            
        }
    }
}
